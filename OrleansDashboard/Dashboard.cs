using Nancy.Hosting.Self;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class Dashboard : IBootstrapProvider
    {
        NancyHost host;
        Logger logger;

        public static DashboardCounters Counters { get; set; }

        public static int HistoryLength
        {
            get
            {
                return 25;
            }
        }

        public string Name
        {
            get
            {
                return "Dashboard";
            }
        }

        public static TaskScheduler OrleansTS { get; private set; }

        public Task Close()
        {
            host.Stop();
            host.Dispose();
            return TaskDone.Done;
        }

        public static IProviderRuntime ProviderRuntime {get;private set;}

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {

            OrleansTS = TaskScheduler.Current;

            Counters = new DashboardCounters();

            ProviderRuntime = providerRuntime;

            logger = providerRuntime.GetLogger("Dashboard");

            var port = config.Properties.ContainsKey("port") ? int.Parse(config.Properties["port"]) : 8080;
            var url = $"http://localhost:{port}";

            host = new NancyHost(new Uri(url));
            host.Start();

            logger.Verbose($"Dashboard listening on {url}");

            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);
            await dashboardGrain.Init();
        }
    }
}
