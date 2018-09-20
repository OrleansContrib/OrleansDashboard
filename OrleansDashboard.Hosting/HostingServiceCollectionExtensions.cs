using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using OrleansDashboard.Client;
using OrleansDashboard.Dispatchers;

namespace OrleansDashboard.Hosting
{
    public static class HostingServiceCollectionExtensions
    {
        internal static string ToValue(this PathString path)
        {
            return path.ToString().Substring(1);
        }

        public static ISiloHostBuilder UseDashboardSelfHosting(this ISiloHostBuilder builder,
            Action<HostingOptions> configurator = null)
        {
            builder.ConfigureApplicationParts(appParts =>
                appParts.AddFrameworkPart(typeof(HostingStartupTask).Assembly).WithReferences().WithCodeGeneration());
            builder.ConfigureServices(services => services.Configure(configurator ?? (x => { })));
            builder.AddStartupTask<HostingStartupTask>();
            return builder;
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app,
            HostingOptions options = null)
        {
            if (string.IsNullOrEmpty(options?.BasePath) || options.BasePath == "/")
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

        public static IServiceCollection AddServicesForSelfHostedDashboard(this IServiceCollection services,
            IClusterClient client = null,
            Action<HostingOptions> configurator = null)
        {
            if (client != null) services.AddSingleton(client);

            services.Configure(configurator ?? (x => { }));
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IExternalDispatcher, ClientDispatcher>();
            services.AddSingleton<IGrainFactory>(c => c.GetRequiredService<IClusterClient>());

            return services;
        }
    }
}