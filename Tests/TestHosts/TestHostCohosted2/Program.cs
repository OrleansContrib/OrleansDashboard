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
using TestGrains;

namespace TestHostCohosted2
{
    class Program
    {
        static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddServicesForSelfHostedDashboard(null, options =>
                    {
                        options.HideTrace = true;
                    });

                    services.AddOrleans(context.Configuration, context.HostingEnvironment, builder =>
                    {
                        var siloPort = 11111;
                        var siloAddress = IPAddress.Loopback;

                        int gatewayPort = 30000;

                        builder.UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort));
                        builder.UseInMemoryReminderService();
                        builder.ConfigureEndpoints(siloAddress, siloPort, gatewayPort);
                        builder.Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "helloworldcluster";
                            options.ServiceId = "1";
                        });

                        builder.ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(TestCalls).Assembly));

                        builder.UseDashboard(options =>
                        {
                            options.HostSelf = false;
                        });
                    });
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .Configure(app =>
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
