using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Client;
using OrleansDashboard.Client.Model;
using OrleansDashboard.Metrics.TypeFormatting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OrleansDashboard.Metrics
{
    public sealed class GrainProfiler : IGrainProfiler, IDisposable
    {
        private readonly Timer timer;
        private readonly ILogger<GrainProfiler> logger;
        private readonly ILocalSiloDetails localSiloDetails;
        private readonly IGrainFactory grainFactory;
        private ConcurrentDictionary<string, SiloGrainTraceEntry> grainTrace = new ConcurrentDictionary<string, SiloGrainTraceEntry>();
        private string siloAddress;
        private IDashboardGrain dashboardGrain;

        public GrainProfiler(IGrainFactory grainFactory, ILogger<GrainProfiler> logger, ILocalSiloDetails localSiloDetails)
        {
            this.grainFactory = grainFactory;

            this.logger = logger;
            this.localSiloDetails = localSiloDetails;

            // register timer to report every second
            timer = new Timer(ProcessStats, null, 1 * 1000, 1 * 1000);
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        public void Track(double elapsedMs, Type grainType, [CallerMemberName] string methodName = null, bool failed = false)
        {
            if (grainType == null)
            {
                throw new ArgumentNullException(nameof(grainType));
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));
            }

            var grainName = grainType.FullName;

            var key = $"{grainName}.{methodName}";

            var exceptionCount = (failed ? 1 : 0);

            grainTrace.AddOrUpdate(key, _ =>
                new SiloGrainTraceEntry
                {
                    Count = 1,
                    ExceptionCount = exceptionCount,
                    ElapsedTime = elapsedMs,
                    Grain = grainName,
                    Method = methodName
                },
            (_, last) =>
            {
                last.Count += 1;
                last.ElapsedTime += elapsedMs;

                if (failed)
                {
                    last.ExceptionCount += exceptionCount;
                }

                return last;
            });
        }

        private void ProcessStats(object state)
        {
            var currentTrace = Interlocked.Exchange(ref grainTrace, new ConcurrentDictionary<string, SiloGrainTraceEntry>());

            if (currentTrace.Count > 0)
            {
                if (siloAddress == null)
                {
                    siloAddress = localSiloDetails.SiloAddress.ToParsableString();
                }

                var items = currentTrace.Values.ToArray();

                foreach (var item in items)
                {
                    item.Grain = TypeFormatter.Parse(item.Grain);
                }

                try
                {
                    dashboardGrain = dashboardGrain ?? grainFactory.GetGrain<IDashboardGrain>(0);

                    dashboardGrain.SubmitTracing(siloAddress, items.AsImmutable()).Ignore();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(100001, ex, "Exception thrown sending tracing to dashboard grain");
                }
            }
        }
    }
}
