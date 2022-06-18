using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using System.Net;
using System.Threading.Tasks;
using TestGrains;

namespace TestHostSeparate
{
    public static class Program
    {
        private static readonly int GatewayPort = 30000;
        private static readonly int SiloPort = 11111;
        private static readonly IPAddress SiloAddress = IPAddress.Loopback;

        public static async Task Main(string[] args)
        {
            //
            // In this sample we just integrate the Dashboard middleware into the client application.
            // 
            var siloHost =
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
                            options.HostSelf = false;
                        });
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IHostedService, TestGrainsHostedService>();
                    })
                    .Build();

            await siloHost.StartAsync();

            await Task.Delay(1000);

            Host.CreateDefaultBuilder(args)
                .UseOrleansClient(builder =>
                {
                    builder.UseStaticClustering(options => options.Gateways.Add((new IPEndPoint(SiloAddress, GatewayPort)).ToGatewayUri()));
                    builder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "helloworldcluster";
                        options.ServiceId = "1";
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddServicesForSelfHostedDashboard();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.Configure(app =>
                    {
                        app.UseOrleansDashboard();

                        app.Map("/dashboard", d =>
                        {
                            d.UseOrleansDashboard();
                        });
                    });
                })
                .Build()
                .Run();

            await siloHost.StopAsync();

        }
    }
}
