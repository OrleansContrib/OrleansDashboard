using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace OrleansDashboard
{
    public class Dashboard : IBootstrapProvider
    {
        private IWebHost host;
        private Logger logger;
        private GrainProfiler profiler;

        private DashboardTraceListener dashboardTraceListener;

        public static int HistoryLength => 100;

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
                host?.Dispose();
            }
            catch { }

            try
            {
                profiler?.Dispose();
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

            var port = config.Properties.ContainsKey("Port") ? int.Parse(config.Properties["Port"]) : 8080;

            var username = config.Properties.ContainsKey("Username") ? config.Properties["Username"] : null;
            var password = config.Properties.ContainsKey("Password") ? config.Properties["Password"] : null;

            try
            {
                var builder = new WebHostBuilder()
                    .ConfigureServices(s => s
                        .AddSingleton(TaskScheduler.Current)
                        .AddSingleton(providerRuntime)
                        .AddSingleton(dashboardTraceListener)
                        .AddSingleton(new UserCredentials(username, password))
                    )
                    .ConfigureServices(services =>
                    {
                        services
                            .AddMvcCore()
                            .AddApplicationPart(typeof(DashboardController).Assembly)
                            .AddJsonFormatters();
                    })
                    .Configure(app =>
                    {
                        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            app.UseMiddleware<BasicAuthMiddleware>();
                        }

                        app.UseMvc();
                    })
                    .UseKestrel()
                    .UseUrls($"http://localhost:{port}");
                host = builder.Build();
                host.Start();
            }
            catch (Exception ex)
            {
                this.logger.Error(10001, ex.ToString());
            }

            this.logger.Verbose($"Dashboard listening on {port}");

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