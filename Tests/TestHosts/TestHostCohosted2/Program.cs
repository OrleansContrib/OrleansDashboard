using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;

namespace TestHostCohosted2
{
    class Program
    {
        private static readonly int GatewayPort = 30000;
        private static readonly int SiloPort = 11111;
        private static readonly IPAddress SiloAddress = IPAddress.Loopback;

        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseOrleans((_, builder) =>
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

                    builder.UseDashboardEmbeddedFiles();
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
        }
    }
}
