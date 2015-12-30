using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Orleans.Providers;
using Orleans.Runtime.Configuration;
using Orleans;

namespace OrleansDashboard
{

    public class StatsPublisher : IConfigurableStatisticsPublisher, IConfigurableSiloMetricsDataPublisher, IConfigurableClientMetricsDataPublisher, IProvider
    {
        public string Name
        {
            get
            {
                return "Dashboard";
            }
        }

        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
        }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            return TaskDone.Done;
        }

        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            return TaskDone.Done;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            return TaskDone.Done;
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            return TaskDone.Done;
        }

        public Task ReportMetrics(IClientPerformanceMetrics metricsData)
        {
            ClientPerformanceMetrics = metricsData;
            return TaskDone.Done;
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            SiloPerformanceMetrics = metricsData;
            return TaskDone.Done;
        }

        public Task ReportStats(List<ICounter> statsCounters)
        {
            Counters = statsCounters;
            return TaskDone.Done;
        }

        public static ISiloPerformanceMetrics SiloPerformanceMetrics { get; private set; }

        public static List<ICounter> Counters { get; private set; }

        public static IClientPerformanceMetrics ClientPerformanceMetrics { get; private set; }


    }
}
