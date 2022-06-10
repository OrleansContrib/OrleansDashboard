using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using TestGrains;

namespace TestHost
{
    public static class Program
    {
        private static readonly int GatewayPort = 30000;
        private static readonly int SiloPort = 11111;
        private static readonly IPAddress SiloAddress = IPAddress.Loopback;

        public static void Main(string[] args)
        {
            //
            // In this sample we let the dashboard host kestrel and the backend services.
            // 
            Host.CreateDefaultBuilder(args)
                .UseOrleans(builder =>
                {
                    builder.UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(SiloAddress, SiloPort));
                    builder.UseInMemoryReminderService();
                    builder.AddMemoryGrainStorageAsDefault();
                    builder.ConfigureEndpoints(SiloAddress, SiloPort, GatewayPort);
                    builder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "helloworldcluster";
                        options.ServiceId = "1";
                    });

                    builder.UseDashboard(options =>
                    {
                        options.HostSelf = true;
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IHostedService, TestGrainsHostedService>();
                })
                .Build()
                .Run();
        }
    }
}
