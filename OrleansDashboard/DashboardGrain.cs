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

            this.Counters.Hosts = hostsTask.Result.ToDictionary(k => k.Key.ToParsableString(), v => v.Value.ToString());
            this.Counters.SimpleGrainStats = simpleGrainStatsTask.Result.Select(x => new SimpleGrainStatisticCounter {
                ActivationCount = x.ActivationCount,
                GrainType = x.GrainType,
                SiloAddress = x.SiloAddress.ToParsableString()
            }).ToArray();
        }

        public override Task OnActivateAsync()
        {
            this.Counters = new DashboardCounters();
            this.RegisterTimer(this.Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
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
    }
}
