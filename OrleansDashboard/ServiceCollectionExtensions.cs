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
            builder.ConfigureApplicationParts(appParts => appParts.AddFrameworkPart(typeof(Dashboard).Assembly).WithReferences().WithCodeGeneration());
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
                else
                {
                    return c.GetRequiredService<SiloStatusOracleSiloDetailsProvider>();
                }
            });
            services.AddSingleton(GrainProfiler.DefaultGrainMethodFormatter);

            return services;
        }

        public static IClientBuilder UseDashboard(this IClientBuilder builder)
        {
            builder.ConfigureApplicationParts(appParts => appParts.AddFrameworkPart(typeof(Dashboard).Assembly).WithReferences().WithCodeGeneration());

            return builder;
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app, DashboardOptions options = null)
        {
            if (options == null || string.IsNullOrEmpty(options.BasePath) || options.BasePath == "/")
            {
                app.UseMiddleware<DashboardMiddleware>();
            }
            else
            {
                //Make sure there is a leading slash
                var basePath = options.BasePath.StartsWith("/") ? options.BasePath : "/" + options.BasePath;
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
            }

            services.Configure(configurator ?? (x => { }));
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IExternalDispatcher, ClientDispatcher>();
            services.AddSingleton<IGrainFactory>(c => c.GetRequiredService<IClusterClient>());

            return services;
        }

        internal static IServiceCollection AddServicesForHostedDashboard(this IServiceCollection services, IGrainFactory grainFactory, IExternalDispatcher dispatcher, DashboardOptions options)
        {
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton(Options.Create(options));
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton(dispatcher);
            services.AddSingleton(grainFactory);

            return services;
        }
    }
}
