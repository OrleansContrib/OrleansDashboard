using Microsoft.Owin.Hosting;
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
        IDisposable host;
        Logger logger;
        DashboardModule module;

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


        public Task Close()
        {
            host.Dispose();
            return TaskDone.Done;
        }


        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.logger = providerRuntime.GetLogger("Dashboard");

            module = new DashboardModule(WebServer.Router, TaskScheduler.Current,  providerRuntime);

            var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = config.Properties.ContainsKey("port") ? int.Parse(config.Properties["port"]) : 8080
            };

            host = WebApp.Start<WebServer>(options);

            this.logger.Verbose($"Dashboard listening on {options.Port}");

            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);
            return dashboardGrain.Init();
        }
    }
}
