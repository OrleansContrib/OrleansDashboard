using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Statistics;
using OrleansDashboard.Hosting;
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
                new SiloHostBuilder()
                    .UseDashboard()
                    .UseDashboardSelfHosting(options =>
                    {
                        options.Port = 8000;
                        options.HideTrace = false;
                    })
                    .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
                    .UseInMemoryReminderService()
                    .UsePerfCounterEnvironmentStatistics()
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
                    .ConfigureServices(services =>
                    {
                        // Workaround for https://github.com/dotnet/orleans/issues/4129
                        services.AddSingleton(cp => cp.GetRequiredService<IHostEnvironmentStatistics>() as ILifecycleParticipant<ISiloLifecycle>);
                    })
                    .Build();

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