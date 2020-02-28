using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TestGrains;

namespace TestHostCohosted
{
    public sealed class SiloHost : IHostedService
    {
        private readonly ISiloHost siloHost;

        public IGrainFactory GrainFactory
        {
            get { return siloHost.Services.GetRequiredService<IGrainFactory>(); }
        }

        public SiloHost()
        {
            var siloPort = 11111;
            var siloAddress = IPAddress.Loopback;

            int gatewayPort = 30000;

            siloHost =
                new SiloHostBuilder()
                    //.UseDashboard(options =>
                    //{
                    //    options.HostSelf = false;
                    //})
                    .UseDashboardCollect()
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
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await siloHost.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return siloHost.StopAsync();
        }
    }
}
