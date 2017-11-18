using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using OrleansDashboard;

// ReSharper disable CheckNamespace

namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ClusterConfiguration RegisterDashboard(this ClusterConfiguration config)
        {
            config.Globals.RegisterBootstrapProvider<Dashboard>("Dashboard", new Dictionary<string, string>());
            config.Globals.RegisterStatisticsProvider<StatsPublisher>("DashboardStats");

            return config;
        }

        public static ISiloHostBuilder UseDashboard(this ISiloHostBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.AddApplicationPartsFromReferences(typeof(Dashboard).Assembly);
            builder.ConfigureServices(services => services.AddDashboard(configurator));

            return builder;
        }

        public static IServiceCollection AddDashboard(this IServiceCollection services,
            Action<DashboardOptions> configurator = null)
        {
            services.Configure(configurator ?? (x => { }));
            services.AddGrainCallFilter<GrainProfiler>();
            services.AddSingleton<IExternalDispatcher, SiloDispatcher>();

            return services;
        }

        public static IClientBuilder UseDashboard(this IClientBuilder builder)
        {
            builder.AddApplicationPartsFromReferences(typeof(Dashboard).Assembly);

            return builder;
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app)
        {
            app.UseMiddleware<DashboardMiddleware>();

            return app;
        }

        public static IServiceCollection AddOrleansDashboardClient(this IServiceCollection services, IClusterClient client = null)
        {
            if (client != null)
            {
                services.AddSingleton(client);
            }

            var logger = new DashboardLogger();

            services.AddSingleton<ILoggerProvider>(logger);
            services.AddSingleton(logger);
            services.AddSingleton<IExternalDispatcher, ClientDispatcher>();
            services.AddSingleton<IGrainFactory>(c => c.GetRequiredService<IClusterClient>());

            return services;
        }

        public static IServiceCollection AddOrleansDashboardSilo(this IServiceCollection services, IGrainFactory grainFactory)
        {
            var logger = new DashboardLogger();

            services.AddSingleton<ILoggerProvider>(logger);
            services.AddSingleton(logger);
            services.AddSingleton<IExternalDispatcher, SiloDispatcher>();
            services.AddSingleton(grainFactory);

            return services;
        }
    }
}
