using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace OrleansDashboard
{
    public class Dashboard : IBootstrapProvider
    {
        private IWebHost host;

        public static int HistoryLength => 100;

        public string Name { get; private set; }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            var options = providerRuntime.ServiceProvider.GetRequiredService<IOptions<DashboardOptions>>();

            if (options.Value.HostSelf)
            {
                var logger = providerRuntime.ServiceProvider.GetRequiredService<ILogger<Dashboard>>();

                try
                {
                    host =
                        new WebHostBuilder()
                            .ConfigureServices(services =>
                            {
                                services.AddServicesForHostedDashboard(providerRuntime.GrainFactory, options);
                            })
                            .Configure(app =>
                            {
                                if (options.Value.HasUsernameAndPassword())
                                {
                                    // only when usename and password are configured
                                    // do we inject basicauth middleware in the pipeline
                                    app.UseMiddleware<BasicAuthMiddleware>();
                                }

                                app.UseOrleansDashboard();
                            })
                            .UseKestrel()
                            .UseUrls($"http://{options.Value.Host}:{options.Value.Port}")
                            .Build();

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

            await ActivateDashboardGrainAsync(providerRuntime);
            await ActivateSiloGrainAsync(providerRuntime);
        }

        private static async Task ActivateSiloGrainAsync(IProviderRuntime providerRuntime)
        {
            var siloGrain = providerRuntime.GrainFactory.GetGrain<ISiloGrain>(providerRuntime.ToSiloAddress());

            await siloGrain.SetOrleansVersion(typeof(SiloAddress).GetTypeInfo().Assembly.GetName().Version.ToString());
        }

        private static async Task ActivateDashboardGrainAsync(IProviderRuntime providerRuntime)
        {
            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            await dashboardGrain.Init();
        }

        public Task Close()
        {
            try
            {
                host?.Dispose();
            }
            catch
            {
                /* NOOP */
            }

            try
            {
                // Teardown the silo dispatcher to prevent deadlocks.
                SiloDispatcher.Teardown();
            }
            catch
            {
                /* NOOP */
            }

            return Task.CompletedTask;
        }
    }
}