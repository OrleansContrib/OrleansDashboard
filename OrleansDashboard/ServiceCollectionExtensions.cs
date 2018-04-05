using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansDashboard;

// ReSharper disable CheckNamespace

namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloHostBuilder UseDashboard(this ISiloHostBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(Dashboard).Assembly));
            builder.ConfigureServices(services => services.AddDashboard(configurator));
            builder.AddStartupTask<Dashboard>();
            builder.AddIncomingGrainCallFilter<GrainProfiler>();

            return builder;
        }

        public static IServiceCollection AddDashboard(this IServiceCollection services,
            Action<DashboardOptions> configurator = null)
        {
            services.Configure(configurator ?? (x => { }));
            services.AddSingleton<SiloStatusOracleSiloDetailsProvider>();
            services.AddSingleton<MembershipTableSiloDetailsProvider>();
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IExternalDispatcher, SiloDispatcher>();
            services.AddSingleton<ITelemetryProducer, DashboardTelemetryProducer>();
            services.AddSingleton<ISiloDetailsProvider>(c =>
            {
                var membershipTable = c.GetService<IMembershipTable>();

                if (membershipTable != null)
                {
                    return c.GetRequiredService<MembershipTableSiloDetailsProvider>();
                }
                else
                {
                    return c.GetRequiredService<SiloStatusOracleSiloDetailsProvider>();
                }
            });

            return services;
        }

        public static IClientBuilder UseDashboard(this IClientBuilder builder)
        {
            builder.ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(Dashboard).Assembly));

            return builder;
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app)
        {
            app.UseMiddleware<DashboardMiddleware>();

            return app;
        }

        public static IServiceCollection AddServicesForSelfHostedDashboard(this IServiceCollection services, IClusterClient client = null,
            Action<DashboardOptions> configurator = null)
        {
            if (client != null)
            {
                services.AddSingleton(client);
            }

            services.Configure(configurator ?? (x => { }));
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IExternalDispatcher, ClientDispatcher>();
            services.AddSingleton<IGrainFactory>(c => c.GetRequiredService<IClusterClient>());

            return services;
        }

        internal static IServiceCollection AddServicesForHostedDashboard(this IServiceCollection services, IGrainFactory grainFactory, DashboardOptions options)
        {
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton(Options.Create(options));
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IExternalDispatcher, SiloDispatcher>();
            services.AddSingleton(grainFactory);

            return services;
        }
    }
}
