using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Implementation;

namespace OrleansDashboard
{
    public sealed class Dashboard : IStartupTask, IDisposable
    {
        private IWebHost host;
        private MeterProvider meterProvider;
        private readonly ILogger<Dashboard> logger;
        private readonly ILocalSiloDetails localSiloDetails;
        private readonly IGrainFactory grainFactory;
        private readonly DashboardTelemetryExporter dashboardTelemetryExporter;
        private readonly DashboardOptions dashboardOptions;

        public Dashboard(
            ILogger<Dashboard> logger,
            ILocalSiloDetails localSiloDetails,
            IGrainFactory grainFactory,
            DashboardTelemetryExporter dashboardTelemetryExporter,
            IOptions<DashboardOptions> dashboardOptions)
        {
            this.logger = logger;
            this.grainFactory = grainFactory;
            this.localSiloDetails = localSiloDetails;
            this.dashboardTelemetryExporter = dashboardTelemetryExporter;
            this.dashboardOptions = dashboardOptions.Value;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            if (dashboardOptions.HostSelf)
            {
                try
                {
                    host =
                        new WebHostBuilder()
                            .ConfigureServices(services =>
                            {
                                services.AddServicesForHostedDashboard(grainFactory, dashboardOptions);
                                services.AddCors(options => {
                                    options.AddDefaultPolicy(policy => policy
                                        .WithOrigins("http://localhost:3000")
                                        .AllowAnyHeader()
                                        .WithMethods("GET")
                                        .Build());
                                });
                                
                            })
                            .Configure(app =>
                            {
                                app.UseCors();

                                if (dashboardOptions.HasUsernameAndPassword())
                                {
                                    // only when username and password are configured
                                    // do we inject basicauth middleware in the pipeline
                                    app.UseMiddleware<BasicAuthMiddleware>();
                                }
                                
                                app.UseOrleansDashboard(dashboardOptions);
                                
                            })
                            .UseKestrel()
                            .UseUrls($"http://{dashboardOptions.Host}:{dashboardOptions.Port}")
                            .Build();

                    await host.StartAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(10001, ex, "Unable to start dashboard host");
                }

                logger.LogInformation($"Dashboard listening on {dashboardOptions.Port}");
            }

            await Task.WhenAll(
                ActivateDashboardGrainAsync(),
                ActivateSiloGrainAsync(),
                StartOpenTelemetryConsumerAsync());
        }

        private async Task ActivateSiloGrainAsync()
        {
            var siloGrain = grainFactory.GetGrain<ISiloGrain>(localSiloDetails.SiloAddress.ToParsableString());

            await siloGrain.SetVersion(GetOrleansVersion(), GetHostVersion());
        }

        private async Task ActivateDashboardGrainAsync()
        {
            var dashboardGrain = grainFactory.GetGrain<IDashboardGrain>(0);

            await dashboardGrain.Init();
        }

        private Task StartOpenTelemetryConsumerAsync()
        {
            meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Orleans")
                .AddReader(new PeriodicExportingMetricReader(dashboardTelemetryExporter, 1000, 1000))
                .Build();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                host?.Dispose();
                meterProvider?.Dispose();
            }
            catch
            {
                /* NOOP */
            }
        }

        private static string GetOrleansVersion()
        {
            var assembly = typeof(SiloAddress).GetTypeInfo().Assembly;
            return string.Format("{0} ({1})",
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                assembly.GetName().Version.ToString());
        }

        private static string GetHostVersion()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly();

                if (assembly != null)
                {
                    return assembly.GetName().Version.ToString();
                }
            }
            catch
            {
                /* NOOP */
            }

            return "1.0.0.0";
        }
    }
}