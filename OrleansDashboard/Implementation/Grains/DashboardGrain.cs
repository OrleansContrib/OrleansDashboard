using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;
using OrleansDashboard.Metrics.Details;
using OrleansDashboard.Metrics.History;
using OrleansDashboard.Metrics.TypeFormatting;
using System.Threading;
using OrleansDashboard.Metrics;
using System.Dynamic;
using System.Reflection;
using Orleans.Core;
using System.Text.Json;
using OrleansDashboard.Implementation.Helpers;
using Orleans.Serialization.TypeSystem;

namespace OrleansDashboard
{
    [Reentrant]
    public class DashboardGrain : Grain, IDashboardGrain
    {
        private readonly ITraceHistory history;
        private readonly ISiloDetailsProvider siloDetailsProvider;
        private readonly DashboardCounters counters;
        private readonly GrainProfilerOptions grainProfilerOptions;
        private readonly TimeSpan updateInterval;
        private bool isUpdating;
        private DateTime startTime = DateTime.UtcNow;
        private DateTime lastRefreshTime = DateTime.UtcNow;
        private DateTime lastQuery = DateTime.UtcNow;
        private bool isEnabled = false;

        public DashboardGrain(
            IOptions<DashboardOptions> options,
            IOptions<GrainProfilerOptions> grainProfilerOptions,
            ISiloDetailsProvider siloDetailsProvider)
        {
            this.siloDetailsProvider = siloDetailsProvider;

            // Store the options to bypass the broadcase of the isEnabled flag.
            this.grainProfilerOptions = grainProfilerOptions.Value;

            // Do not allow smaller timers than 1000ms = 1sec.
            updateInterval = TimeSpan.FromMilliseconds(Math.Max(options.Value.CounterUpdateIntervalMs, 1000));

            // Make the history configurable.
            counters = new DashboardCounters(options.Value.HistoryLength);

            history = new TraceHistoryV2(options.Value.HistoryLength);
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            startTime = DateTime.UtcNow;

            if (!grainProfilerOptions.TraceAlways)
            {
                var interval = TimeSpan.FromMinutes(1);

                RegisterTimer(async x =>
                {
                    var timeSinceLastQuery = DateTimeOffset.UtcNow - lastQuery;

                    if (timeSinceLastQuery > grainProfilerOptions.DeactivationTime && isEnabled)
                    {
                        isEnabled = false;
                        await BroadcaseEnabled();
                    }
                }, null, interval, interval);
            }

            return base.OnActivateAsync(cancellationToken);
        }

        private Task EnsureIsActive()
        {
            lastQuery = DateTime.UtcNow;

            if (!isEnabled)
            {
                isEnabled = true;
                _ = BroadcaseEnabled();
            }

            return Task.CompletedTask;
        }

        private async Task BroadcaseEnabled()
        {
            if (grainProfilerOptions.TraceAlways)
            {
                return;
            }

            var silos = await siloDetailsProvider.GetSiloDetails();

            foreach (var siloAddress in silos.Select(x => x.SiloAddress))
            {
                await GrainFactory.GetGrain<ISiloGrain>(siloAddress).Enable(isEnabled);
            }
        }

        private async Task EnsureCountersAreUpToDate()
        {
            if (isUpdating)
            {
                return;
            }

            var now = DateTime.UtcNow;

            if ((now - lastRefreshTime) < updateInterval)
            {
                return;
            }

            isUpdating = true;
            try
            {
                var metricsGrain = GrainFactory.GetGrain<IManagementGrain>(0);
                var activationCountTask = metricsGrain.GetTotalActivationCount();
                var simpleGrainStatsTask = metricsGrain.GetSimpleGrainStatistics();
                var siloDetailsTask = siloDetailsProvider.GetSiloDetails();
                var detailGrainStatsTask = metricsGrain.GetDetailedGrainStatistics();

                await Task.WhenAll(activationCountTask, simpleGrainStatsTask, siloDetailsTask, detailGrainStatsTask);

                RecalculateCounters(activationCountTask.Result, siloDetailsTask.Result, simpleGrainStatsTask.Result, detailGrainStatsTask.Result);

                lastRefreshTime = now;
            }
            finally
            {
                isUpdating = false;
            }
        }

        internal void RecalculateCounters(int activationCount, SiloDetails[] hosts,
            IList<SimpleGrainStatistic> simpleGrainStatistics, DetailedGrainStatistic[] detailGrainStatistics)
        {
            counters.TotalActivationCount = activationCount;

            counters.TotalActiveHostCount = hosts.Count(x => x.SiloStatus == SiloStatus.Active);
            counters.TotalActivationCountHistory = counters.TotalActivationCountHistory.Enqueue(activationCount).Dequeue();
            counters.TotalActiveHostCountHistory = counters.TotalActiveHostCountHistory.Enqueue(counters.TotalActiveHostCount).Dequeue();

            var elapsedTime = Math.Min((DateTime.UtcNow - startTime).TotalSeconds, 100);

            counters.Hosts = hosts;

            var aggregatedTotals = history.GroupByGrainAndSilo().ToLookup(x => (x.Grain, x.SiloAddress));

            counters.SimpleGrainStats = simpleGrainStatistics.Select(x =>
            {
                var grainName = TypeFormatter.Parse(x.GrainType);
                var siloAddress = x.SiloAddress.ToParsableString();

                var result = new SimpleGrainStatisticCounter
                {
                    ActivationCount = x.ActivationCount,
                    GrainType = grainName,
                    SiloAddress = siloAddress,
                    TotalSeconds = elapsedTime,
                };

                foreach (var item in aggregatedTotals[(grainName, siloAddress)])
                {
                    result.TotalAwaitTime += item.ElapsedTime;
                    result.TotalCalls += item.Count;
                    result.TotalExceptions += item.ExceptionCount;
                }

                return result;
            }).ToArray();
        }

        public async Task<Immutable<DashboardCounters>> GetCounters()
        {
            await EnsureIsActive();
            await EnsureCountersAreUpToDate();

            return counters.AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GetGrainTracing(string grain)
        {
            await EnsureIsActive();
            await EnsureCountersAreUpToDate();

            return history.QueryGrain(grain).AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetClusterTracing()
        {
            await EnsureIsActive();
            await EnsureCountersAreUpToDate();

            return history.QueryAll().AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetSiloTracing(string address)
        {
            await EnsureIsActive();
            await EnsureCountersAreUpToDate();

            return history.QuerySilo(address).AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods(int take)
        {
            await EnsureIsActive();
            await EnsureCountersAreUpToDate();

            var values = history.AggregateByGrainMethod().ToList();

            GrainMethodAggregate[] GetTotalCalls()
            {
                return values.OrderByDescending(x => x.Count).Take(take).ToArray();
            }

            GrainMethodAggregate[] GetLatency()
            {
                return values.OrderByDescending(x => x.Count).Take(take).ToArray();
            }

            GrainMethodAggregate[] GetErrors()
            {
                return values.Where(x => x.ExceptionCount > 0 && x.Count > 0).OrderByDescending(x => x.ExceptionCount / x.Count).Take(take).ToArray();
            }

            var result = new Dictionary<string, GrainMethodAggregate[]>
            {
                { "calls", GetTotalCalls() },
                { "latency", GetLatency() },
                { "errors", GetErrors() },
            };

            return result.AsImmutable();
        }

        public Task Init()
        {
            // just used to activate the grain
            return Task.CompletedTask;
        }

        public Task SubmitTracing(string siloAddress, Immutable<SiloGrainTraceEntry[]> grainTrace)
        {
            history.Add(DateTime.UtcNow, siloAddress, grainTrace.Value);

            return Task.CompletedTask;
        }

        public async Task<Immutable<string>> GetGrainState(string id, string grainType)
        {
            var result = new ExpandoObject();

            try
            {
                var implementationType = GrainStateHelper.GetGrainType(grainType);

                var mappedGrainId = GrainStateHelper.GetGrainId(id, implementationType);
                object grainId = mappedGrainId.Item1;
                string keyExtension = mappedGrainId.Item2;

                var propertiesAndFields = GrainStateHelper.GetPropertiesAndFieldsForGrainState(implementationType);

                var getGrainMethod = GrainStateHelper.GenerateGetGrainMethod(GrainFactory, grainId, keyExtension);

                var interfaceTypes = implementationType.GetInterfaces();

                foreach (var interfaceType in interfaceTypes)
                {
                    try
                    {
                        object[] grainMethodParameters;
                        if (string.IsNullOrWhiteSpace(keyExtension))
                            grainMethodParameters = new object[] { interfaceType, grainId };
                        else
                            grainMethodParameters = new object[] { interfaceType, grainId,keyExtension };

                        var grain = getGrainMethod.Invoke(GrainFactory, grainMethodParameters);

                        var methods = interfaceType.GetMethods().Where(w => w.GetParameters().Length == 0);

                        foreach (var method in methods)
                        {
                            try
                            {
                                if (method.ReturnType.IsAssignableTo(typeof(Task))
                                    &&
                                    (
                                        method.ReturnType.GetGenericArguments()
                                                    .Any(a => propertiesAndFields.Any(f => f == a)
                                        || method.Name == "GetState")
                                    )
                                )
                                {
                                    var task = (method.Invoke(grain, null) as Task);
                                    var resultProperty = task.GetType().GetProperty("Result");

                                    if (resultProperty == null)
                                        continue;

                                    await task.ConfigureAwait(false);

                                    result.TryAdd(method.Name, resultProperty.GetValue(task));
                                }
                            }
                            catch
                            {
                                // Because we got all the interfaces some errors with boxing and unboxing may happen with invocations 
                            }
                        }
                    }
                    catch
                    {
                        // Because we got all the interfaces some errors with boxing and unboxing may happen when try to get the grain
                    }
                }
            }
            catch (Exception ex)
            {
                result.TryAdd("error", string.Concat( ex.Message , " - " , ex?.InnerException.Message));
            }

            return JsonSerializer.Serialize(result, options: new JsonSerializerOptions()
            {
                WriteIndented = true,
            }).AsImmutable();
        }

        public Task<Immutable<IEnumerable<string>>> GetGrainTypes()
        {
            return Task.FromResult(GrainStateHelper.GetGrainTypes()
                                     .Select(s => s.Namespace + "." + s.Name)
                                     .AsImmutable());
        }
    }
}