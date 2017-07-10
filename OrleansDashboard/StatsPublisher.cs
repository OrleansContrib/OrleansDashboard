using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{

    public class StatCounter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Delta { get; set; }
    }

    public class StatsPublisher : IConfigurableStatisticsPublisher, IStatisticsPublisher, IProvider, ISiloMetricsDataPublisher
    {
        public string Name { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task ReportStats(List<ICounter> statsCounters)
        {
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<ISiloGrain>(this.ProviderRuntime.ToSiloAddress());
            var values = statsCounters.Select(x => new StatCounter { Name = x.Name, Value = x.GetValueString(), Delta = x.IsValueDelta ? x.GetDeltaString() : null}).OrderBy(x => x.Name).ToArray();
            await Dispatch(async () => {
                await grain.ReportCounters(values);
            });
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        { }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            throw new NotImplementedException();
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.Name = name;
            this.ProviderRuntime = providerRuntime;
            return Task.CompletedTask;
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            throw new NotImplementedException();
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            return Task.CompletedTask;
        }

        Task Dispatch(Func<Task> func)
        {
            var scheduler = Dashboard.OrleansScheduler;
            return scheduler == null 
                ? Task.CompletedTask 
                : Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler);
        }
    }
   
}
