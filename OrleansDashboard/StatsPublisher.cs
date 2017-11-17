using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Providers;
using Orleans.Runtime;

namespace OrleansDashboard
{
    public class StatCounter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Delta { get; set; }
    }

    public class StatsPublisher : IConfigurableStatisticsPublisher, IProvider, ISiloMetricsDataPublisher
    {
        private IExternalDispatcher dispatcher;
        private IProviderRuntime runtime;

        public string Name { get; private set; }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            return Task.CompletedTask;
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
        }

        public async Task ReportStats(List<ICounter> statsCounters)
        {
            if (dispatcher.CanDispatch())
            {
                var grain = runtime.GrainFactory.GetGrain<ISiloGrain>(runtime.ToSiloAddress());

                var values = statsCounters.Select(x => new StatCounter
                {
                    Name = x.Name,
                    Value = x.GetValueString(),
                    Delta = x.IsValueDelta ? x.GetDeltaString() : null
                }).OrderBy(x => x.Name).ToArray();

                await dispatcher.DispatchAsync(() => grain.ReportCounters(values));
            }
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            dispatcher = providerRuntime.ServiceProvider.GetRequiredService<IExternalDispatcher>();
            runtime = providerRuntime;

            return Task.CompletedTask;
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            return Task.CompletedTask;
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            return Task.CompletedTask;
        }
    }
}