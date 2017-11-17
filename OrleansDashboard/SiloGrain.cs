using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class SiloGrain : Grain, ISiloGrain
    {
        Queue<SiloRuntimeStatistics> stats;
        IDisposable timer;

        public string Version { get; private set; }
        public StatCounter[] Counters { get; private set; }

        public override async Task OnActivateAsync()
        {
            stats = new Queue<SiloRuntimeStatistics>();
            Counters = new StatCounter[0];

            foreach (var x in Enumerable.Range(1, Dashboard.HistoryLength))
            {
                stats.Enqueue(null);
            }

            timer = RegisterTimer(Callback, true, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            await Callback(false);

            await base.OnActivateAsync();
        }

        async Task Callback(object canDeactivate)
        {
            var address = SiloAddress.FromParsableString(this.GetPrimaryKeyString());
            var grain = GrainFactory.GetGrain<IManagementGrain>(0);
            try
            {
                var results = (await grain.GetRuntimeStatistics(new SiloAddress[] { address })).FirstOrDefault();
                stats.Enqueue(results);
                while (stats.Count > Dashboard.HistoryLength)
                {
                    stats.Dequeue();
                }
            }
            catch (Exception)
            {
                // we can't get the silo stats, it's probably dead, so kill the grain
                if (!(bool)canDeactivate) return;
                if (null != timer) timer.Dispose();
                timer = null;
                DeactivateOnIdle();
            }
        }

        public Task<SiloRuntimeStatistics[]> GetRuntimeStatistics()
        {
            return Task.FromResult(stats.ToArray());
        }

        public Task SetOrleansVersion(string version)
        {
            Version = version;
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, string>> GetExtendedProperties()
        {
            var results = new Dictionary<string, string>();

            try
            {
                var assembly = Assembly.GetEntryAssembly();
                if (null != assembly)
                {
                    results.Add("HostVersion", assembly.GetName().Version.ToString());
                }
            }
            catch
            { }

            if (null != Version)
            {
                results.Add("OrleansVersion", Version);
            }

            return Task.FromResult(results);
        }

        public Task ReportCounters(StatCounter[] counters)
        {
            Counters = counters;
            return Task.CompletedTask;
        }

        public Task<StatCounter[]> GetCounters()
        {
            return Task.FromResult(Counters);
        }
    }
}
