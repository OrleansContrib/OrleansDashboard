using System;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using TestGrains;

// ReSharper disable MethodSupportsCancellation

namespace TestHostSeparate
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var configuration =
                ClusterConfiguration.LocalhostPrimarySilo(33333)
                    .RegisterDashboard();

            var silo =
                new SiloHostBuilder()
                    .UseConfiguration(configuration)
                    .UseDashboard(options =>
                    {
                        options.HostSelf = false;
                    })
                    .AddApplicationPartsFromReferences(typeof(TestCalls).Assembly)
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                    })
                    .Build();

            silo.StartAsync().Wait();

            var client =
                new ClientBuilder()
                    .UseConfiguration(ClientConfiguration.LocalhostSilo())
                    .UseDashboard()
                    .AddApplicationPartsFromReferences(typeof(TestCalls).Assembly)
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
                    services.AddOrleansDashboardClient(client);
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
                        // d.UseOrleansDashboard();
                    });
                })
                .Build()
                .Run();

            cts.Cancel();

            silo.StopAsync().Wait();
        }
    }
}