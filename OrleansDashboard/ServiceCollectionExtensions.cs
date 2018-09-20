using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansDashboard;
using OrleansDashboard.Client;
using OrleansDashboard.DetailsProviders;
using OrleansDashboard.Dispatchers;

// ReSharper disable CheckNamespace

namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloHostBuilder UseDashboard(this ISiloHostBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.ConfigureApplicationParts(appParts => appParts.AddFrameworkPart(typeof(DashboardStartupTask).Assembly).WithReferences().WithCodeGeneration());
            builder.ConfigureServices(services => services.AddDashboard(configurator));
            builder.AddStartupTask<DashboardStartupTask>();
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
            services.AddSingleton<SiloDispatcher>();
            services.AddSingleton<IExternalDispatcher>(sp => sp.GetRequiredService<SiloDispatcher>());
            services.Configure<TelemetryOptions>(options => options.AddConsumer<DashboardTelemetryConsumer>());
            services.AddSingleton<ISiloDetailsProvider>(c =>
            {
                var membershipTable = c.GetService<IMembershipTable>();

                if (membershipTable != null)
                {
                    return c.GetRequiredService<MembershipTableSiloDetailsProvider>();
                }

                return c.GetRequiredService<SiloStatusOracleSiloDetailsProvider>();
            });
            services.AddSingleton(GrainProfiler.DefaultGrainMethodFormatter);

            return services;
        }

        public static IClientBuilder UseDashboard(this IClientBuilder builder)
        {
            builder.ConfigureApplicationParts(appParts => appParts.AddFrameworkPart(typeof(DashboardStartupTask).Assembly).WithReferences().WithCodeGeneration());

            return builder;
        }
    }
}
