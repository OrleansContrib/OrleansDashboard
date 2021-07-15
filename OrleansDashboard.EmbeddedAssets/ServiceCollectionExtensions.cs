using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using OrleansDashboard.EmbeddedAssets;
using OrleansDashboard.Implementation.Assets;

// ReSharper disable CheckNamespace

namespace Orleans
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloBuilder UseDashboardEmbeddedFiles(this ISiloBuilder builder)
        {
            builder.ConfigureServices(services => services.AddDashboardEmbeddedFiles());

            return builder;
        }

        public static IServiceCollection AddDashboardEmbeddedFiles(this IServiceCollection services)
        {
            services.AddSingleton<IAssetProvider, EmbeddedAssetProvider>();

            return services;
        }
    }
}
