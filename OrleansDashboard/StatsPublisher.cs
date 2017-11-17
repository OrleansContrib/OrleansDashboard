using System.Collections.Generic;
using System.Globalization;
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
    }

    public class StatsPublisher : IProvider, IStatisticsPublisher, ISiloMetricsDataPublisher
    {
        private IExternalDispatcher dispatcher;
        private IProviderRuntime runtime;

        public string Name { get; private set; }

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

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            return Task.CompletedTask;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task ReportStats(List<ICounter> statsCounters)
        {
            if (dispatcher.CanDispatch())
            {
                var grain = runtime.GrainFactory.GetGrain<ISiloGrain>(runtime.ToSiloAddress());

                var values = statsCounters.Select(x => new StatCounter
                {
                    Name = x.Name,
                    Value = x.GetValueString()
                }).OrderBy(x => x.Name).ToArray();

                await dispatcher.DispatchAsync(() => grain.ReportCounters(values));
            }
        }

        public async Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            if (dispatcher.CanDispatch())
            {
                var counters = new List<StatCounter>();

                void AddValue(string key, object value)
                {
                    var name = char.ToLowerInvariant(key[0]) + key.Substring(1);

                    counters.Add(new StatCounter { Name = name, Value = string.Format(CultureInfo.InvariantCulture, "{0}", value) });
                };

                AddValue(nameof(metricsData.ActivationCount), metricsData.ActivationCount);
                AddValue(nameof(metricsData.AvailablePhysicalMemory), metricsData.AvailablePhysicalMemory);
                AddValue(nameof(metricsData.ClientCount), metricsData.ClientCount);
                AddValue(nameof(metricsData.CpuUsage), metricsData.CpuUsage);
                AddValue(nameof(metricsData.IsOverloaded), metricsData.IsOverloaded);
                AddValue(nameof(metricsData.MemoryUsage), metricsData.MemoryUsage);
                AddValue(nameof(metricsData.ReceivedMessages), metricsData.ReceivedMessages);
                AddValue(nameof(metricsData.ReceiveQueueLength), metricsData.ReceiveQueueLength);
                AddValue(nameof(metricsData.RecentlyUsedActivationCount), metricsData.RecentlyUsedActivationCount);
                AddValue(nameof(metricsData.RequestQueueLength), metricsData.RequestQueueLength);
                AddValue(nameof(metricsData.SendQueueLength), metricsData.SendQueueLength);
                AddValue(nameof(metricsData.SentMessages), metricsData.SentMessages);
                AddValue(nameof(metricsData.TotalPhysicalMemory), metricsData.TotalPhysicalMemory);

                var grain = runtime.GrainFactory.GetGrain<ISiloGrain>(runtime.ToSiloAddress());

                await dispatcher.DispatchAsync(() => grain.ReportCounters(counters.ToArray()));
            }
        }
    }
}