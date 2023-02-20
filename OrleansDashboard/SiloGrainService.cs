using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
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
        private readonly Channel<SiloRuntimeStatistics> statisticsChannel;
        private readonly Dictionary<string, StatCounter> counters = new();
        private readonly DashboardOptions options;
        private readonly IGrainProfiler profiler;
        private readonly IGrainFactory grainFactory;
        private IDisposable timer;
        private string versionOrleans;
        private string versionHost;

        public SiloGrainService(
            IGrainProfiler profiler,
            IOptions<DashboardOptions> options,
            IGrainFactory grainFactory)
        {
            this.profiler = profiler;
            this.options = options.Value;
            this.grainFactory = grainFactory;
            statisticsChannel = Channel.CreateBounded<SiloRuntimeStatistics>(
                new BoundedChannelOptions(options.Value.HistoryLength)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.DropOldest,
                    AllowSynchronousContinuations = true,
                }
            );
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

                await statisticsChannel.Writer.WriteAsync(results);
            }
            catch (Exception)
            {
                // we can't get the silo stats, it's probably dead, so kill the grain
                if (canDeactivate)
                {
                    timer?.Dispose();
                    timer = null;
                    statisticsChannel.Writer.TryComplete();
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

        public Task<Immutable<List<SiloRuntimeStatistics>>> GetRuntimeStatistics()
        {
            var statisticsCount = statisticsChannel.Reader.Count;
            var result = new List<SiloRuntimeStatistics>(statisticsCount);
            var i = 0;
            while (i++ < statisticsCount && statisticsChannel.Reader.TryRead(out var statistics))
            {
                result.Add(statistics);
            }

            return Task.FromResult(result.AsImmutable());
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