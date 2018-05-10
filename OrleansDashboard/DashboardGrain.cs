using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using OrleansDashboard.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    [Reentrant]
    [PreferLocalPlacement]
    public class DashboardGrain : Grain, IDashboardGrain
    {
        private DashboardCounters Counters { get; set; }
        private DateTime StartTime { get; set; }
        private ITraceHistory history = new TraceHistory();
        private ISiloDetailsProvider siloDetailsProvider;
        private IDisposable timer;

        private async Task Callback(object _)
        {
            var metricsGrain = this.GrainFactory.GetGrain<IManagementGrain>(0);
            var activationCountTask = metricsGrain.GetTotalActivationCount();
            var simpleGrainStatsTask = metricsGrain.GetSimpleGrainStatistics();
            var siloDetailsTask = siloDetailsProvider.GetSiloDetails();

            await Task.WhenAll(activationCountTask,  simpleGrainStatsTask, siloDetailsTask);

            RecalculateCounters(activationCountTask.Result, siloDetailsTask.Result, simpleGrainStatsTask.Result);
        }

        internal void RecalculateCounters(int activationCount, SiloDetails[] hosts,
            IList<SimpleGrainStatistic> simpleGrainStatistics)
        {
            this.Counters.TotalActivationCount = activationCount;

            this.Counters.TotalActiveHostCount = hosts.Count(x => x.SiloStatus == SiloStatus.Active);
            this.Counters.TotalActivationCountHistory.Enqueue(activationCount);
            this.Counters.TotalActiveHostCountHistory.Enqueue(this.Counters.TotalActiveHostCount);

            while (this.Counters.TotalActivationCountHistory.Count > Dashboard.HistoryLength)
            {
                this.Counters.TotalActivationCountHistory.Dequeue();
            }
            while (this.Counters.TotalActiveHostCountHistory.Count > Dashboard.HistoryLength)
            {
                this.Counters.TotalActiveHostCountHistory.Dequeue();
            }

            // TODO - whatever max elapsed time
            var elapsedTime = Math.Min((DateTime.UtcNow - this.StartTime).TotalSeconds, 100);

            this.Counters.Hosts = hosts;

            var aggregatedTotals = history.GroupByGrainAndSilo().ToLookup(x => (x.Grain, x.SiloAddress));

            this.Counters.SimpleGrainStats = simpleGrainStatistics.Select(x =>
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
            // note: normally we would use dependency injection
            // but since we do not have access to the registered services collection 
            // from within a bootstrapper we do it this way:
            // first try to resolve from the container, if not present in container
            // then instantiate the default
            this.siloDetailsProvider =
                (this.ServiceProvider.GetService(typeof(ISiloDetailsProvider)) as ISiloDetailsProvider)
                ?? new MembershipTableSiloDetailsProvider(this.GrainFactory);


            this.Counters = new DashboardCounters();
            this.StartTime = DateTime.UtcNow;
            return base.OnActivateAsync();
        }

        public Task<DashboardCounters> GetCounters()
        {
            return Task.FromResult(this.Counters);
        }

        public Task<Dictionary<string, Dictionary<string, GrainTraceEntry>>> GetGrainTracing(string grain)
        {
            return Task.FromResult(history.QueryGrain(grain));
        }

        public Task<Dictionary<string, GrainTraceEntry>> GetClusterTracing()
        {
            return Task.FromResult(this.history.QueryAll());
        }

        public Task<Dictionary<string, GrainTraceEntry>> GetSiloTracing(string address)
        {
            return Task.FromResult(this.history.QuerySilo(address));
        }

        public Task Init(DashboardGrainSettings settings)
        {
            if (null == settings) throw new ArgumentNullException(nameof(settings));

            // just used to activate the grain
            if (null == this.timer)
            {
                this.timer = this.RegisterTimer(this.Callback, null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(settings.GrainSampleFrequncyMs));
            }

            return Task.CompletedTask;
        }

        public Task SubmitTracing(string siloAddress, SiloGrainTraceEntry[] grainTrace)
        {
            history.Add(DateTime.UtcNow, siloAddress, grainTrace);

            return Task.CompletedTask;
        }
    }
}