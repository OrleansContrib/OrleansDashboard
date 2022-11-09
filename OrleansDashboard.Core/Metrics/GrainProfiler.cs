using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Model;
using OrleansDashboard.Metrics.TypeFormatting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Serialization.TypeSystem;

namespace OrleansDashboard.Metrics
{
    public sealed class GrainProfiler : IGrainProfiler, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly ILogger<GrainProfiler> logger;
        private readonly ILocalSiloDetails localSiloDetails;
        private readonly IOptions<GrainProfilerOptions> options;
        private readonly IGrainFactory grainFactory;
        private ConcurrentDictionary<string, SiloGrainTraceEntry> grainTrace = new ConcurrentDictionary<string, SiloGrainTraceEntry>();
        private Timer timer;
        private string siloAddress;
        private bool isEnabled;
        private IDashboardGrain dashboardGrain;

        public bool IsEnabled
        {
            get => options.Value.TraceAlways || isEnabled;
        }

        public GrainProfiler(IGrainFactory grainFactory, ILogger<GrainProfiler> logger, ILocalSiloDetails localSiloDetails, IOptions<GrainProfilerOptions> options)
        {
            this.grainFactory = grainFactory;
            this.logger = logger;
            this.localSiloDetails = localSiloDetails;
            this.options = options;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe<GrainProfiler>(ServiceLifecycleStage.Last, ct => OnStart(), ct => OnStop());
        }

        private Task OnStart()
        {
            timer = new Timer(ProcessStats, null, 1 * 1000, 1 * 1000);

            return Task.CompletedTask;
        }

        private Task OnStop()
        {
            timer.Dispose();

            return Task.CompletedTask;
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

            if (!IsEnabled)
            {
                return;
            }

            // This is the method that Orleans uses to convert a grain type into the grain type name when calling the GetSimpleGrainStatistics method
            var grainName = RuntimeTypeNameFormatter.Format(grainType);
            var grainMethodKey = $"{grainName}.{methodName}";

            var exceptionCount = (failed ? 1 : 0);

            grainTrace.AddOrUpdate(grainMethodKey, _ =>
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
            if (!IsEnabled)
            {
                return;
            }

            var currentTrace = Interlocked.Exchange(ref grainTrace, new ConcurrentDictionary<string, SiloGrainTraceEntry>());

            if (!currentTrace.IsEmpty)
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
                    dashboardGrain ??= grainFactory.GetGrain<IDashboardGrain>(0);

                    dashboardGrain.SubmitTracing(siloAddress, items.AsImmutable()).Ignore();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(100001, ex, "Exception thrown sending tracing to dashboard grain");
                }
            }
        }

        public void Enable(bool enabled)
        {
            isEnabled = enabled;
        }
    }
}
