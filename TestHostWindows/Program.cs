﻿using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Statistics;
using TestGrains;

// ReSharper disable MethodSupportsCancellation

namespace TestHostWindows
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
                    .UseDashboard(options =>
                    {
                        options.HostSelf = true;
                        options.HideTrace = false;
                    })
                    .UsePerfCounterEnvironmentStatistics()
                    .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
                    .UseInMemoryReminderService()
                    .ConfigureEndpoints(siloAddress, siloPort, gatewayPort)
                    .Configure(options => options.ClusterId = "helloworldcluster")
                    .ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(TestCalls).Assembly))
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                    })
                    .Build();

            silo.StartAsync().Wait();

            var client =
                new ClientBuilder()
                    .UseStaticClustering(options => options.Gateways.Add((new IPEndPoint(siloAddress, gatewayPort)).ToGatewayUri()))
                    .ConfigureCluster(options => options.ClusterId = "helloworldcluster")
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