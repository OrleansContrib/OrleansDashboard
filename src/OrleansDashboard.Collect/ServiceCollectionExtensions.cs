using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.ApplicationParts;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansDashboard;
using OrleansDashboard.Metrics;
using OrleansDashboard.Metrics.Details;
using System;

namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloHostBuilder UseDashboardCollect(this ISiloHostBuilder builder)
        {
            builder.ConfigureApplicationParts(parts => parts.AddDashboardParts());
            builder.ConfigureServices(services =>
            {
                services.Configure<DashboardCollectOptions>(x => { });
                services.AddDashboardCollect();
            });
            return builder;
        }
        public static ISiloHostBuilder UseDashboardCollect(this ISiloHostBuilder builder, Action<DashboardCollectOptions> configurator)
        {
            builder.UseDashboardCollect();
            builder.ConfigureServices(services => services.Configure(configurator));
            return builder;
        }
        public static ISiloHostBuilder UseDashboardCollect(this ISiloHostBuilder builder, DashboardCollectOptions options)
        {
            builder.UseDashboardCollect();
            builder.ConfigureServices(services=>services.AddSingleton(Options.Create(options)));
            return builder;
        }

        public static ISiloBuilder UseDashboardCollect(this ISiloBuilder builder)
        {
            builder.ConfigureApplicationParts(parts => parts.AddDashboardParts());
            builder.ConfigureServices(services =>
            {
                services.Configure<DashboardCollectOptions>(x => { });
                services.AddDashboardCollect();
            });
            return builder;
        }
        public static ISiloBuilder UseDashboardCollect(this ISiloBuilder builder, DashboardCollectOptions options)
        {
            builder.UseDashboardCollect();
            builder.ConfigureServices(services => services.AddSingleton(Options.Create(options)));
            return builder;
        }
        public static ISiloBuilder UseDashboardCollect(this ISiloBuilder builder, Action<DashboardCollectOptions> configurator)
        {
            builder.UseDashboardCollect();
            builder.ConfigureServices(services => services.Configure(configurator));
            return builder;
        }

        public static IServiceCollection AddDashboardCollect(this IServiceCollection services)
        {
            services.AddSingleton<SiloStatusOracleSiloDetailsProvider>();
            services.AddSingleton<MembershipTableSiloDetailsProvider>();
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IGrainProfiler, GrainProfiler>();
            services.AddSingleton(c => (ILifecycleParticipant<ISiloLifecycle>)c.GetRequiredService<IGrainProfiler>());
            services.AddSingleton<IIncomingGrainCallFilter, GrainProfilerFilter>();
            services.AddSingleton<ISiloDetailsProvider>(c =>
            {
                var membershipTable = c.GetService<IMembershipTable>();
                if (membershipTable != null)
                {
                    return c.GetRequiredService<MembershipTableSiloDetailsProvider>();
                }
                return c.GetRequiredService<SiloStatusOracleSiloDetailsProvider>();
            });

            services.TryAddSingleton(GrainProfilerFilter.NoopOldGrainMethodFormatter);
            services.TryAddSingleton(GrainProfilerFilter.DefaultGrainMethodFormatter);

            return services;
        }

        public static void AddDashboardParts(this IApplicationPartManager appParts)
        {
            appParts.AddFrameworkPart(typeof(DashboardGrain).Assembly).WithReferences();
        }

    }
}
