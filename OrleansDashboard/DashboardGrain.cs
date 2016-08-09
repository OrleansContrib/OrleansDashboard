using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    [Reentrant]
    public class DashboardGrain : Grain, IDashboardGrain
    {
        DashboardCounters Counters { get; set; }
        DateTime StartTime { get; set; }

        async Task Callback(object _)
        {

            // purge old silos from the stats
            var retirementWindow = DateTime.UtcNow.AddSeconds(-1000);
            foreach (var item in this.GrainTracing.ToArray())
            {
                if (item.Value.LastUpdated <= retirementWindow) continue;
                //this.GrainTracing.Remove(item.Key);
            }


            var metricsGrain = this.GrainFactory.GetGrain<IManagementGrain>(0);
            var activationCountTask = metricsGrain.GetTotalActivationCount();
            var hostsTask = metricsGrain.GetHosts(true);
            var simpleGrainStatsTask = metricsGrain.GetSimpleGrainStatistics();

            await Task.WhenAll(activationCountTask, hostsTask, simpleGrainStatsTask);

            this.Counters.TotalActivationCount = activationCountTask.Result;
            this.Counters.TotalActiveHostCount = hostsTask.Result.Values.Count(x => x == SiloStatus.Active);
            this.Counters.TotalActivationCountHistory.Enqueue(activationCountTask.Result);
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
            var elapsedTime = Math.Min((DateTime.UtcNow - this.StartTime).TotalSeconds, 30);

            this.Counters.Hosts = hostsTask.Result.ToDictionary(k => k.Key.ToParsableString(), v => v.Value.ToString());
            this.Counters.SimpleGrainStats = simpleGrainStatsTask.Result.Select(x => new SimpleGrainStatisticCounter {
                ActivationCount = x.ActivationCount,
                GrainType = x.GrainType,
                SiloAddress = x.SiloAddress.ToParsableString(),
                TotalAwaitTime = this.GrainTracing.ContainsKey(x.SiloAddress.ToParsableString()) ? this.GrainTracing[x.SiloAddress.ToParsableString()].Trace.SumZero(y => y.Values.Where(z => z.Grain == x.GrainType).SumZero(b => b.ElapsedTime)) : 0,
                TotalCalls = this.GrainTracing.ContainsKey(x.SiloAddress.ToParsableString()) ? this.GrainTracing[x.SiloAddress.ToParsableString()].Trace.SumZero(y => y.Values.Where(z => z.Grain == x.GrainType).SumZero(b => b.Count)) : 0,
                TotalSeconds = elapsedTime
            }).ToArray();
        }

        public override Task OnActivateAsync()
        {
            this.Counters = new DashboardCounters();
            this.RegisterTimer(this.Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
            this.StartTime = DateTime.UtcNow;
            return base.OnActivateAsync();
        }

        public Task<DashboardCounters> GetCounters()
        {
            return Task.FromResult(this.Counters);
        }

        public Task Init()
        {
            // just used to activate the grain
            return TaskDone.Done;
        }

        class SiloTracingEntry
        {
            public DateTime LastUpdated { get; set; }
            public string Silo { get; set; }
            public IDictionary<string, GrainTraceEntry>[] Trace { get; set; }
        }

        IDictionary<string, SiloTracingEntry> GrainTracing = new Dictionary<string, SiloTracingEntry>();

        public Task SubmitTracing(string siloIdentity, IDictionary<string, GrainTraceEntry>[] grainCallTime)
        {
            SiloTracingEntry entry = null;
            if (this.GrainTracing.ContainsKey(siloIdentity))
            {
                entry = this.GrainTracing[siloIdentity];
            }
            else
            {
                entry = new SiloTracingEntry
                {
                    Silo = siloIdentity
                };
                this.GrainTracing.Add(siloIdentity, entry);
            }

            entry.LastUpdated = DateTime.UtcNow;
            entry.Trace = grainCallTime;

            return TaskDone.Done;
        }
    }
}
