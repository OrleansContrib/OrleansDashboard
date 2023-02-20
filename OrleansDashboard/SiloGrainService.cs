using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Metrics;
using OrleansDashboard.Model;

namespace OrleansDashboard
{
    public sealed class SiloGrainService : GrainService, ISiloGrainService
    {
        private const int DefaultTimerIntervalMs = 1000; // 1 second
        private readonly Queue<SiloRuntimeStatistics> statistics;
        private readonly Dictionary<string, StatCounter> counters = new();
        private readonly DashboardOptions options;
        private readonly IGrainProfiler profiler;
        private readonly IGrainFactory grainFactory;
        private IDisposable timer;
        private string versionOrleans;
        private string versionHost;

        public SiloGrainService(
            GrainId grainId,
            Silo silo,
            ILoggerFactory loggerFactory,
            IGrainProfiler profiler,
            IOptions<DashboardOptions> options,
            IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
        {
            this.profiler = profiler;
            this.options = options.Value;
            this.grainFactory = grainFactory;
            statistics = new Queue<SiloRuntimeStatistics>(this.options.HistoryLength + 1);
        }

        public override async Task Start()
        {
            var updateInterval =
                TimeSpan.FromMilliseconds(Math.Max(options.CounterUpdateIntervalMs, DefaultTimerIntervalMs));
            try
            {
                timer = RegisterTimer(x => CollectStatistics((bool) x), true, updateInterval, updateInterval);

                await CollectStatistics(false);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Not running in Orleans runtime");
            }

            await base.Start();
        }

        private async Task CollectStatistics(bool canDeactivate)
        {
            var managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
            try
            {
                var siloAddress = SiloAddress.FromParsableString(this.GetPrimaryKeyString());

                var results = (await managementGrain.GetRuntimeStatistics(new[] {siloAddress})).FirstOrDefault();

                statistics.Enqueue(results);

                while (statistics.Count > options.HistoryLength)
                {
                    statistics.Dequeue();
                }
            }
            catch (Exception)
            {
                // we can't get the silo stats, it's probably dead, so kill the grain
                if (canDeactivate)
                {
                    timer?.Dispose();
                    timer = null;
                }
            }
        }

        public Task SetVersion(string orleans, string host)
        {
            versionOrleans = orleans;
            versionHost = host;

            return Task.CompletedTask;
        }

        public Task<Immutable<Dictionary<string, string>>> GetExtendedProperties()
        {
            var results = new Dictionary<string, string>
            {
                ["HostVersion"] = versionHost,
                ["OrleansVersion"] = versionOrleans
            };

            return Task.FromResult(results.AsImmutable());
        }

        public Task ReportCounters(Immutable<StatCounter[]> reportCounters)
        {
            foreach (var counter in reportCounters.Value)
            {
                if (!string.IsNullOrWhiteSpace(counter.Name))
                {
                    counters[counter.Name] = counter;
                }
            }

            return Task.CompletedTask;
        }

        public Task<Immutable<SiloRuntimeStatistics[]>> GetRuntimeStatistics()
        {
            return Task.FromResult(statistics.ToArray().AsImmutable());
        }

        public Task<Immutable<StatCounter[]>> GetCounters()
        {
            return Task.FromResult(counters.Values.OrderBy(x => x.Name).ToArray().AsImmutable());
        }

        public Task Enable(bool enabled)
        {
            profiler.Enable(enabled);

            return Task.CompletedTask;
        }
    }
}