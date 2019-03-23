using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TestHostCohosted2
{
    public static class Extensions
    {
        public static IServiceCollection AddOrleans(this IServiceCollection services, IConfiguration config, IWebHostEnvironment environment, Action<ISiloBuilder> builder)
        {
            var hostBuilder = new SiloServiceBuilder(config, environment);

            builder?.Invoke(hostBuilder);

            hostBuilder.Build(services);

            return services;
        }
    }
}
