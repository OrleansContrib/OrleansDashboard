using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansDashboard;
using OrleansDashboard.Metrics;
using OrleansDashboard.Metrics.Details;
using System;

// ReSharper disable CheckNamespace

namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloHostBuilder UseDashboard(this ISiloHostBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.AddStartupTask<Dashboard>();
            builder.ConfigureServices(services => services.Configure(configurator ?? (x => { })));
            if (configurator == null)
                builder.UseDashboardCollect();
            else
            {
                var opt = new DashboardOptions();
                configurator.Invoke(opt);
                builder.UseDashboardCollect(opt);
            }
            return builder;
        }

        public static ISiloBuilder UseDashboard(this ISiloBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.AddStartupTask<Dashboard>();
            builder.ConfigureServices(services => services.Configure(configurator ?? (x => { })));
            if (configurator == null)
                builder.UseDashboardCollect();
            else
            {
                var opt = new DashboardOptions();
                configurator.Invoke(opt);
                builder.UseDashboardCollect(opt);
            }
            return builder;
        }

        public static IClientBuilder UseDashboard(this IClientBuilder builder)
        {
            builder.ConfigureApplicationParts(parts => parts.AddDashboardParts());
            return builder;
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app, DashboardOptions options = null)
        {
            if (string.IsNullOrEmpty(options?.BasePath) || options.BasePath == "/")
            {
                app.UseMiddleware<DashboardMiddleware>();
            }
            else
            {
                // Make sure there is a leading slash
                var basePath = options.BasePath.StartsWith("/") ? options.BasePath : $"/{options.BasePath}";

                app.Map(basePath, a => a.UseMiddleware<DashboardMiddleware>());
            }
            return app;
        }

        public static IServiceCollection AddServicesForSelfHostedDashboard(this IServiceCollection services, IClusterClient client = null,
            Action<DashboardOptions> configurator = null)
        {
            if (client != null)
            {
                services.AddSingleton(client);
                services.AddSingleton<IGrainFactory>(c => c.GetRequiredService<IClusterClient>());
            }
            services.Configure(configurator ?? (x => { }));
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            return services;
        }

        internal static IServiceCollection AddServicesForHostedDashboard(this IServiceCollection services, IGrainFactory grainFactory, DashboardOptions options)
        {
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton(Options.Create(options));
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton(grainFactory);
            return services;
        }
    }
}
