using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;

namespace TestHostCohosted
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddServicesForSelfHostedDashboard(null, options =>
                    {
                        options.HideTrace = true;
                    });
                    
                    services.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "helloworldcluster";
                        options.ServiceId = "1";
                    });
                })
                .UseOrleans(options =>
                {
                    var siloPort = 11111;
                    var siloAddress = IPAddress.Loopback;

                    int gatewayPort = 30000;

                    options
                        .UseDashboard(options =>
                         {
                             options.HostSelf = false;
                         })
                        .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
                        .UseInMemoryReminderService()
                        .ConfigureEndpoints(siloAddress, siloPort, gatewayPort);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .ConfigureWebHost(app =>
                {
                    app.UseOrleansDashboard();

                    app.Map("/dashboard", d =>
                    {
                        d.UseOrleansDashboard();
                    });
                })
                .Build()
                .Run();
        }
    }
}
