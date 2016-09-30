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
            var elapsedTime = Math.Min((DateTime.UtcNow - this.StartTime).TotalSeconds, 100);

            this.Counters.Hosts = hostsTask.Result.ToDictionary(k => k.Key.ToParsableString(), v => v.Value.ToString());
            this.Counters.SimpleGrainStats = simpleGrainStatsTask.Result.Select(x => new SimpleGrainStatisticCounter {
                ActivationCount = x.ActivationCount,
                GrainType = x.GrainType,
                SiloAddress = x.SiloAddress.ToParsableString(),
                TotalAwaitTime = this.history.Where(n => n.Grain == x.GrainType && n.SiloAddress == x.SiloAddress.ToParsableString()).SumZero(n => n.ElapsedTime),
                TotalCalls = this.history.Where(n => n.Grain == x.GrainType && n.SiloAddress == x.SiloAddress.ToParsableString()).SumZero(n => 1),
                TotalSeconds = elapsedTime
            }).ToArray();
        }

        public override Task OnActivateAsync()
        {
            this.Counters = new DashboardCounters();
            this.RegisterTimer(this.Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
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
         
            foreach (var historicValue in this.history.Where(x => x.Grain == grain))
            {
                var grainMethodKey = $"{grain}.{historicValue.Method}";
                if (!results.ContainsKey(grainMethodKey))
                {
                    results.Add(grainMethodKey, new Dictionary<string, GrainTraceEntry>());
                }
                var grainResults = results[grainMethodKey];

                var key = historicValue.Period.ToString("o");
                if (!grainResults.ContainsKey(key)) grainResults.Add(key, new GrainTraceEntry
                {
                    Grain = historicValue.Grain,
                    Method = historicValue.Method,
                    Period = historicValue.Period
                });
                var value = grainResults[key];
                value.Count += historicValue.Count;
                value.ElapsedTime += historicValue.ElapsedTime;
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

            // fill in any previously captured methods which aren't in this reporting window
            var allGrainTrace = new List<GrainTraceEntry>(grainTrace);
            var values = this.history.Where(x => x.SiloAddress == siloIdentity).GroupBy(x => x.GrainAndMethod).Select(x => x.First());
            foreach (var value in values)
            {
                if (!grainTrace.Any(x => x.GrainAndMethod == value.GrainAndMethod))
                {
                    allGrainTrace.Add(new GrainTraceEntry
                    {
                        Count = 0,
                        ElapsedTime = 0,
                        Grain = value.Grain,
                        Method = value.Method,
                        Period = now,
                        SiloAddress = siloIdentity
                    });
                }
            }

            var retirementWindow = DateTime.UtcNow.AddSeconds(-100);
            history.AddRange(allGrainTrace);
            history.RemoveAll(x => x.Period < retirementWindow);

            return TaskDone.Done;
        }
    }
}
