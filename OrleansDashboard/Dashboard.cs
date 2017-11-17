using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Providers;
using Orleans.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace OrleansDashboard
{
    public class Dashboard : IBootstrapProvider
    {
        private IWebHost host;
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

            OrleansScheduler = null;

            return Task.CompletedTask;
        }

        public static TaskScheduler OrleansScheduler { get; private set; }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.Name = name;

            var options = providerRuntime.ServiceProvider.GetRequiredService < IOptions<DashboardOptions>>();
            var logger = providerRuntime.ServiceProvider.GetRequiredService<ILogger<Dashboard>>();

            this.dashboardTraceListener = new DashboardTraceListener();

            if (options.Value.HostSelf)
            {
                try
                {
                    var builder = new WebHostBuilder()
                        .ConfigureServices(s => s
                            .AddSingleton(TaskScheduler.Current)
                            .AddSingleton(providerRuntime)
                            .AddSingleton(dashboardTraceListener)
                        )
                        .ConfigureServices(services =>
                        {
                            services
                                .AddMvcCore()
                                .AddApplicationPart(typeof(DashboardController).GetTypeInfo().Assembly)
                                .AddJsonFormatters();
                        })
                        .Configure(app =>
                        {
                            if (options.Value.HasUsernameAndPassword())
                            {
                                // only when usename and password are configured
                                // do we inject basicauth middleware in the pipeline
                                app.UseMiddleware<BasicAuthMiddleware>();
                            }

                            app.UseMvc();
                        })
                        .UseKestrel()
                        .UseUrls($"http://{options.Value.Host}:{options.Value.Port}");
                    host = builder.Build();
                    host.Start();
                }
                catch (Exception ex)
                {
                    logger.Error(10001, ex.ToString());
                }

                logger.LogInformation($"Dashboard listening on {options.Value.Port}");
            }

            // horrible hack to grab the scheduler
            // to allow the stats publisher to push
            // counters to grains
            SiloDispatcher.Setup();

            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);
            await dashboardGrain.Init();

            var siloGrain = providerRuntime.GrainFactory.GetGrain<ISiloGrain>(providerRuntime.ToSiloAddress());
            await siloGrain.SetOrleansVersion(typeof(SiloAddress).GetTypeInfo().Assembly.GetName().Version.ToString());
            Trace.Listeners.Add(dashboardTraceListener);

            OrleansScheduler = TaskScheduler.Current;
        }
    }
}