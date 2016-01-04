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

        public static IProviderRuntime ProviderRuntime { get; private set; }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {

            Dashboard.OrleansTS = TaskScheduler.Current;
            Dashboard.ProviderRuntime = providerRuntime;

            this.logger = providerRuntime.GetLogger("Dashboard");

            var port = config.Properties.ContainsKey("port") ? int.Parse(config.Properties["port"]) : 8080;
            var url = $"http://localhost:{port}";

            this.host = new NancyHost(new Uri(url));
            this.host.Start();

            this.logger.Verbose($"Dashboard listening on {url}");

            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);
            return dashboardGrain.Init();
        }
    }
}
