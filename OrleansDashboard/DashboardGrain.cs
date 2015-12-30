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
    [StatelessWorker]
    public class DashboardGrain : Grain, IDashboardGrain
    {

        async Task Callback(object _)
        {

            var metricsGrain = this.GrainFactory.GetGrain<IManagementGrain>(0);
            var activationCountTask = metricsGrain.GetTotalActivationCount();
            var hostsTask = metricsGrain.GetHosts();
            var simpleGrainStatsTask = metricsGrain.GetSimpleGrainStatistics();

            await Task.WhenAll(activationCountTask, hostsTask, simpleGrainStatsTask);

            Dashboard.Counters.TotalActivationCount = activationCountTask.Result;
            Dashboard.Counters.TotalActiveHostCount = hostsTask.Result.Values.Count(x => x == SiloStatus.Active);
            Dashboard.Counters.TotalActivationCountHistory.Enqueue(activationCountTask.Result);
            Dashboard.Counters.TotalActiveHostCountHistory.Enqueue(Dashboard.Counters.TotalActiveHostCount);
            
            while (Dashboard.Counters.TotalActivationCountHistory.Count > Dashboard.HistoryLength)
            {
                Dashboard.Counters.TotalActivationCountHistory.Dequeue();
            }
            while (Dashboard.Counters.TotalActiveHostCountHistory.Count > Dashboard.HistoryLength)
            {
                Dashboard.Counters.TotalActiveHostCountHistory.Dequeue();
            }

            Dashboard.Counters.Hosts = hostsTask.Result.ToDictionary(k => k.Key.ToParsableString(), v => v.Value.ToString());
            Dashboard.Counters.SimpleGrainStats = simpleGrainStatsTask.Result.Select(x => new SimpleGrainStatisticCounter {
                ActivationCount = x.ActivationCount,
                GrainType = x.GrainType,
                SiloAddress = x.SiloAddress.ToParsableString()
            }).ToArray();
        }

        public Task Init()
        {
            this.RegisterTimer(this.Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
            return base.OnActivateAsync();
        }
    }
}
