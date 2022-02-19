using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using TestGrains;

// ReSharper disable MethodSupportsCancellation

namespace TestHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var siloPort = 11111;
            int gatewayPort = 30000;
            var siloAddress = IPAddress.Loopback;

            var silo =
                new ServiceCollection()
                    .AddOrleans(orleans =>
                    {
                        orleans
                            .UseDashboard(options =>
                             {
                                 options.HostSelf = true;
                                 options.HideTrace = false;
                             })
                            .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
                            .UseInMemoryReminderService()
                            .ConfigureEndpoints(siloAddress, siloPort, gatewayPort)
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "helloworldcluster";
                                options.ServiceId = "1";
                            })
                            .ConfigureLogging(builder =>
                            {
                                builder.AddConsole();
                            });
                    })
                    .BuildServiceProvider()
                    .GetRequiredService<IISi>;

            silo.StartAsync().Wait();

            var client =
                new ClientBuilder()
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

            Console.WriteLine("Press key to exit...");
            Console.ReadLine();

            cts.Cancel();

            silo.StopAsync().Wait();
        }
    }
}