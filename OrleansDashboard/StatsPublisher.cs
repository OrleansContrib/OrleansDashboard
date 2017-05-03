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
    }

    public class StatsPublisher : IConfigurableStatisticsPublisher, IStatisticsPublisher, IProvider, ISiloMetricsDataPublisher
    {
        public string Name { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }
        public TaskScheduler Scheduler { get; private set; }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public async Task ReportStats(List<ICounter> statsCounters)
        {
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<ISiloGrain>(this.ProviderRuntime.ToSiloAddress());
            var values = statsCounters.Select(x => new StatCounter { Name = x.Name, Value = x.GetDisplayString() }).ToArray();
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
            this.Scheduler = TaskScheduler.Current;
            return TaskDone.Done;
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            throw new NotImplementedException();
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            return TaskDone.Done;
        }

        Task Dispatch(Func<Task> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.Scheduler);
        }
    }
   
}
