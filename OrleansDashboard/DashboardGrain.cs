using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using OrleansDashboard.History;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OrleansDashboard.Client;
using OrleansDashboard.Client.Model;
using OrleansDashboard.Client.Model.History;

namespace OrleansDashboard
{
    [Reentrant]
    [PreferLocalPlacement]
    public class DashboardGrain : Grain, IDashboardGrain
    {
        const int DefaultTimerIntervalMs = 1000; // 1 second
        private static readonly TimeSpan DefaultTimerInterval = TimeSpan.FromSeconds(1);
        private readonly ITraceHistory history = new TraceHistory();
        private readonly DashboardOptions options;
        private readonly ISiloDetailsProvider siloDetailsProvider;
        private readonly DashboardCounters counters = new DashboardCounters();
        private DateTime startTime = DateTime.UtcNow;

        public DashboardGrain(IOptions<DashboardOptions> options, ISiloDetailsProvider siloDetailsProvider)
        {
            this.options = options.Value;
            this.siloDetailsProvider = siloDetailsProvider;
        }
        
        private async Task Callback(object _)
        {
            var metricsGrain = GrainFactory.GetGrain<IManagementGrain>(0);
            var activationCountTask = metricsGrain.GetTotalActivationCount();
            var simpleGrainStatsTask = metricsGrain.GetSimpleGrainStatistics();
            var siloDetailsTask = siloDetailsProvider.GetSiloDetails();

            await Task.WhenAll(activationCountTask,  simpleGrainStatsTask, siloDetailsTask);

            RecalculateCounters(activationCountTask.Result, siloDetailsTask.Result, simpleGrainStatsTask.Result);
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
            //var aggregatedTotals = history.ToLookup(x => (x.Grain, x.SiloAddress));

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

        public override Task OnActivateAsync()
        {
            var updateInterval =  TimeSpan.FromMilliseconds(Math.Max(options.CounterUpdateIntervalMs, DefaultTimerIntervalMs));
       
            try
            {
                RegisterTimer(Callback, null, updateInterval, updateInterval);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Not running in Orleans runtime");
            }

            startTime = DateTime.UtcNow;

            return base.OnActivateAsync();
        }

        public Task<Immutable<DashboardCounters>> GetCounters()
        {
            return Task.FromResult(counters.AsImmutable());
        }

        public Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GetGrainTracing(string grain)
        {
            return Task.FromResult(history.QueryGrain(grain).AsImmutable());
        }

        public Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetClusterTracing()
        {
            return Task.FromResult(this.history.QueryAll().AsImmutable());
        }

        public Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetSiloTracing(string address)
        {
            return Task.FromResult(this.history.QuerySilo(address).AsImmutable());
        }

        public Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods()
        {
            const int numberOfResultsToReturn = 5;
            
            var values = history.AggregateByGrainMethod().ToList();
            
            return Task.FromResult(new Dictionary<string, GrainMethodAggregate[]>{
                { "calls", values.OrderByDescending(x => x.Count).Take(numberOfResultsToReturn).ToArray() },
                { "latency", values.OrderByDescending(x => x.ElapsedTime / (double) x.Count).Take(numberOfResultsToReturn).ToArray() },
                { "errors", values.Where(x => x.ExceptionCount > 0 && x.Count > 0).OrderByDescending(x => x.ExceptionCount / x.Count).Take(numberOfResultsToReturn).ToArray() },
            }.AsImmutable());
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
