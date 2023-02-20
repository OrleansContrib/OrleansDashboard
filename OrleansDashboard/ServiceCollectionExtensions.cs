using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansDashboard;
using OrleansDashboard.Implementation;
using OrleansDashboard.Implementation.Assets;
using OrleansDashboard.Implementation.Details;
using OrleansDashboard.Metrics;
using OrleansDashboard.Metrics.Details;

// ReSharper disable CheckNamespace
namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloBuilder UseDashboard(this ISiloBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.AddPlacementDirector<LocalPlacementStrategy, LocalPlacementDirector>();

            builder.ConfigureServices(services => services.AddDashboard(configurator));
            builder.AddStartupTask<Dashboard>();

            return builder;
        }

        public static IServiceCollection AddDashboard(this IServiceCollection services,
            Action<DashboardOptions> configurator = null)
        {
            services.Configure(configurator ?? (x => { }));
            services.AddSingleton<DashboardTelemetryExporter>();
            services.AddOptions<GrainProfilerOptions>();

            services.AddSingleton<SiloStatusOracleSiloDetailsProvider>();
            services.AddSingleton<MembershipTableSiloDetailsProvider>();
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IGrainProfiler, GrainProfiler>();
            services.AddSingleton(c => (ILifecycleParticipant<ISiloLifecycle>) c.GetRequiredService<IGrainProfiler>());
            services.AddSingleton<IIncomingGrainCallFilter, GrainProfilerFilter>();
            services.TryAddSingleton<IAssetProvider, CDNAssetProvider>();

            services.AddGrainService<SiloGrainService>()
                .AddSingleton<ISiloGrainClient, SiloGrainClient>();

            services.AddSingleton<ISiloDetailsProvider>(c =>
            {
                var membershipTable = c.GetService<IMembershipTable>();

                if (membershipTable != null)
                {
                    return c.GetRequiredService<MembershipTableSiloDetailsProvider>();
                }

                return c.GetRequiredService<SiloStatusOracleSiloDetailsProvider>();
            });

            services.TryAddSingleton(GrainProfilerFilter.DefaultGrainMethodFormatter);

            return services;
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app,
            DashboardOptions options = null)
        {
            var basePath = options?.BasePath;

            if (string.IsNullOrEmpty(basePath) || basePath == "/")
            {
                app.UseMiddleware<DashboardMiddleware>();
            }
            else
            {
                // Make sure there is a leading slash                
                if (!basePath.StartsWith("/"))
                {
                    basePath = $"/{options.BasePath}";
                }

                app.Map(basePath, app => { app.UseMiddleware<DashboardMiddleware>(); });
            }

            return app;
        }

        public static IServiceCollection AddServicesForSelfHostedDashboard(this IServiceCollection services,
            Action<DashboardOptions> configurator = null)
        {
            services.Configure(configurator ?? (x => { }));
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.TryAddSingleton<IAssetProvider, CDNAssetProvider>();

            return services;
        }

        internal static IServiceCollection AddServicesForHostedDashboard(this IServiceCollection services,
            IGrainFactory grainFactory, DashboardOptions options)
        {
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton(Options.Create(options));
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton(grainFactory);
            services.TryAddSingleton<IAssetProvider, CDNAssetProvider>();

            return services;
        }
    }
}