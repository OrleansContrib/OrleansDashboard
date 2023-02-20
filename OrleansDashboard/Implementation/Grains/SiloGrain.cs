using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Metrics;
using OrleansDashboard.Model;

namespace OrleansDashboard.Implementation.Grains
{
    [LocalPlacement]
    public sealed class SiloGrain : Grain, ISiloGrain
    {
        private const int DefaultTimerIntervalMs = 1000; // 1 second
        private readonly Channel<SiloRuntimeStatistics> statistics;
        private readonly Dictionary<string, StatCounter> counters = new Dictionary<string, StatCounter>();
        private readonly DashboardOptions options;
        private readonly ILocalSiloDetails silo;
        private readonly IGrainProfiler profiler;
        private IDisposable timer;
        private string versionOrleans;
        private string versionHost;

        public SiloGrain(ILocalSiloDetails silo, IGrainProfiler profiler, IOptions<DashboardOptions> options)
        {
            this.silo = silo;
            this.profiler = profiler;
            this.options = options.Value;
            statistics = Channel.CreateBounded<SiloRuntimeStatistics>(
                new BoundedChannelOptions(options.Value.HistoryLength)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.DropOldest,
                    AllowSynchronousContinuations = true,
                });
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var id = this.GetPrimaryKeyString();

            // Ensure that the grain is not activated on another silo.
            var siloAddress = silo.SiloAddress.ToParsableString();
            if (!string.Equals(id, siloAddress))
            {
                // for now Calling DeactivateOnIdle from within OnActivateAsync is not supported so throw an exception
                throw new InvalidOperationException(
                    $"Silo grain {id} must not be activated on this silo {siloAddress}");
            }

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

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task CollectStatistics(bool canDeactivate)
        {
            var managementGrain = GrainFactory.GetGrain<IManagementGrain>(0);
            try
            {
                var siloAddress = SiloAddress.FromParsableString(this.GetPrimaryKeyString());

                var results = (await managementGrain.GetRuntimeStatistics(new[] {siloAddress})).FirstOrDefault();

                await statistics.Writer.WriteAsync(results);
            }
            catch (Exception)
            {
                // we can't get the silo stats, it's probably dead, so kill the grain
                if (canDeactivate)
                {
                    timer?.Dispose();
                    timer = null;
                    statistics.Writer.TryComplete();

                    DeactivateOnIdle();
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

        public async Task<Immutable<List<SiloRuntimeStatistics>>> GetRuntimeStatistics()
        {
            var result = new List<SiloRuntimeStatistics>(statistics.Reader.Count);
            await foreach (var stat in statistics.Reader.ReadAllAsync(CancellationToken.None))
            {
                result.Add(stat);
            }

            return result.AsImmutable();
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