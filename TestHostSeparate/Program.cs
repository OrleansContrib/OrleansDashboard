using System.Net;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansDashboard.Hosting;
using TestGrains;

// ReSharper disable MethodSupportsCancellation

namespace TestHostSeparate
{
    public static class Program
    {
        // by default this will start the dashboard on http://localhost:5000/dashboard
     
        public static void Main(string[] args)
        {
            var siloPort = 11111;
            int gatewayPort = 30000;
            var siloAddress = IPAddress.Loopback;

            var silo =
                new SiloHostBuilder()
                    .UseDashboard()
                    .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
                    .UseInMemoryReminderService()
                    .ConfigureEndpoints(siloAddress, siloPort, gatewayPort)
                    .Configure<ClusterOptions>(options => 
                    {
                        options.ClusterId = "helloworldcluster";
                        options.ServiceId = "1";
                    })
                    .ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(TestCalls).Assembly))
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                    })
                    .Build();

            silo.StartAsync().Wait();

            var client =
                new ClientBuilder()
                    .UseDashboard()
                    .UseStaticClustering(options => options.Gateways.Add((new IPEndPoint(siloAddress, gatewayPort)).ToGatewayUri()))
                    .Configure<ClusterOptions>(options => 
                    {
                        options.ClusterId = "helloworldcluster";
                        options.ServiceId = "1";
                    })
                    .ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(TestCalls).Assembly))
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                    })
                    .Build();

            client.Connect().Wait();

            var cts = new CancellationTokenSource();

            TestCalls.Make(client, cts);

            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddServicesForSelfHostedDashboard(client, options =>
                    {
                        options.HideTrace = true;
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

            cts.Cancel();

            silo.StopAsync().Wait();
        }
    }
}