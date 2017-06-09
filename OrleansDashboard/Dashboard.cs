using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace OrleansDashboard
{
    public class Dashboard : IBootstrapProvider
    {
        IDisposable host;
        Logger logger;
        GrainProfiler profiler;
        private DashboardController controller;
        private DashboardTraceListener dashboardTraceListener;

        public static int HistoryLength
        {
            get
            {
                return 100;
            }
        }

        public string Name { get; private set; }
       


        public Task Close()
        {
            try
            {
                Trace.Listeners.Remove(dashboardTraceListener);
            }
            catch { }

            try
            {
                if (null != controller) controller.Dispose();
            }
            catch { }

            try
            {
                if (null != host) host.Dispose();
            }
            catch { }

            try
            {
                if (null != profiler) profiler.Dispose();
            }
            catch { }

            OrleansScheduler = null;

            return TaskDone.Done;
        }


        public static TaskScheduler OrleansScheduler { get; private set; }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.Name = name;
            
            this.logger = providerRuntime.GetLogger("Dashboard");

            this.dashboardTraceListener = new DashboardTraceListener();

            var router = new Router();
            this.controller = new DashboardController(router, TaskScheduler.Current,  providerRuntime, dashboardTraceListener);

            var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = config.Properties.ContainsKey("Port") ? int.Parse(config.Properties["Port"]) : 8080,
            };

            var username = config.Properties.ContainsKey("Username") ? config.Properties["Username"] : null;
            var password = config.Properties.ContainsKey("Password") ? config.Properties["Password"] : null;
            try
            {
                host = WebApp.Start(options, app => new WebServer(router, username, password).Configuration(app));
                Trace.Listeners.Remove("HostingTraceListener");
            }
            catch (Exception ex)
            {
                this.logger.Error(10001, ex.ToString());
            }

            this.logger.Verbose($"Dashboard listening on {options.Port}");


            this.profiler = new GrainProfiler(TaskScheduler.Current, providerRuntime);


            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);
            await dashboardGrain.Init();

            
            var siloGrain = providerRuntime.GrainFactory.GetGrain<ISiloGrain>(providerRuntime.ToSiloAddress());
            await siloGrain.SetOrleansVersion(typeof(SiloAddress).Assembly.GetName().Version.ToString());
            Trace.Listeners.Add(dashboardTraceListener);



            // horrible hack to grab the scheduler 
            // to allow the stats publisher to push 
            // counters to grains
            OrleansScheduler = TaskScheduler.Current;

        }
    }
}
