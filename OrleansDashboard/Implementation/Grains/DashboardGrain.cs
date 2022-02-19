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

namespace OrleansDashboard
{
    [Reentrant]
    public class DashboardGrain : Grain, IDashboardGrain
    {
        private readonly ITraceHistory history = new TraceHistory();
        private readonly ISiloDetailsProvider siloDetailsProvider;
        private readonly DashboardCounters counters;
        private readonly TimeSpan updateInterval;
        private bool isUpdating;
        private DateTime startTime = DateTime.UtcNow;
        private DateTime lastRefreshTime = DateTime.UtcNow;

        public DashboardGrain(IOptions<DashboardOptions> options, ISiloDetailsProvider siloDetailsProvider)
        {
            this.siloDetailsProvider = siloDetailsProvider;

            // Do not allow smaller timers than 1000ms = 1sec.
            updateInterval = TimeSpan.FromMilliseconds(Math.Max(options.Value.CounterUpdateIntervalMs, 1000));

            counters = new DashboardCounters(options.Value.HistoryLength);
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

                await Task.WhenAll(activationCountTask, simpleGrainStatsTask, siloDetailsTask);

                RecalculateCounters(activationCountTask.Result, siloDetailsTask.Result, simpleGrainStatsTask.Result);

                lastRefreshTime = now;
            }
            finally
            {
                isUpdating = false;
            }
        }

        internal void RecalculateCounters(int activationCount, SiloDetails[] hosts,
            IList<SimpleGrainStatistic> simpleGrainStatistics)
        {
            counters.TotalActivationCount = activationCount;

            counters.TotalActiveHostCount = hosts.Count(x => x.SiloStatus == SiloStatus.Active);
            counters.TotalActivationCountHistory = counters.TotalActivationCountHistory.Enqueue(activationCount).Dequeue();
            counters.TotalActiveHostCountHistory = counters.TotalActiveHostCountHistory.Enqueue(counters.TotalActiveHostCount).Dequeue();

            // TODO - whatever max elapsed time
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
                    TotalSeconds = elapsedTime
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

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            startTime = DateTime.UtcNow;

            return base.OnActivateAsync(cancellationToken);
        }

        public async Task<Immutable<DashboardCounters>> GetCounters()
        {
            await EnsureCountersAreUpToDate();

            return counters.AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GetGrainTracing(string grain)
        {
            await EnsureCountersAreUpToDate();

            return history.QueryGrain(grain).AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetClusterTracing()
        {
            await EnsureCountersAreUpToDate();

            return history.QueryAll().AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetSiloTracing(string address)
        {
            await EnsureCountersAreUpToDate();

            return history.QuerySilo(address).AsImmutable();
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods(int take)
        {
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
    }
}
