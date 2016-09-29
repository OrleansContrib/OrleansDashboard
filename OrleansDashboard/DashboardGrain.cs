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
                TotalAwaitTime = this.history.Where(n => n.GrainType === x.GrainType && n.SiloAddress == x.SiloAddress).SumZero(n => n.),
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
    
        public Task<Dictionary<string,Dictionary<string, GrainTraceEntry>>> GetGrainTracing(string grain)
        {
            var results = new Dictionary<string,Dictionary<string,GrainTraceEntry>>();

            // for each silo
            foreach (var stats in this.GrainTracing.Values.Select(x => x.Trace))
            {
                foreach (var stat in stats)
                {
                    
                    foreach (var keyValue in stat.Where(x => x.Value.Grain == grain))
                    {
                        var grainMethodKey = $"{grain}.{keyValue.Value.Method}";
                        if (!results.ContainsKey(grainMethodKey))
                        {
                            results.Add(grainMethodKey, new Dictionary<string, GrainTraceEntry>());
                        }
                        var grainResults = results[grainMethodKey];

                        var key = keyValue.Value.Period.ToString("o");
                        if (!grainResults.ContainsKey(key)) grainResults.Add(key, new GrainTraceEntry
                        {
                            Grain = keyValue.Value.Grain,
                            Method = keyValue.Value.Method,
                            Period = keyValue.Value.Period
                        });
                        var value = grainResults[key];
                        value.Count += keyValue.Value.Count;
                        value.ElapsedTime += keyValue.Value.ElapsedTime;
                    }
                }
            }
            return Task.FromResult(results);
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


        List<GrainTraceEntry> history = new List<GrainTraceEntry>();

        public Task SubmitTracing(string siloIdentity, GrainTraceEntry[] grainTrace)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in grainTrace)
            {
                // sync clocks
                entry.Period = now;
            }
            var retirementWindow = DateTime.UtcNow.AddSeconds(-1000);
            history.AddRange(grainTrace);
            history.RemoveAll(x => x.Period < retirementWindow);

            return TaskDone.Done;
        }
    }
}
