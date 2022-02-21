using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace OrleansDashboard.Metrics
{
    public class GrainProfilerFilter : IIncomingGrainCallFilter
    {
        public delegate string GrainMethodFormatterDelegate(IIncomingGrainCallContext callContext);

        public static readonly GrainMethodFormatterDelegate DefaultGrainMethodFormatter = c => c.ImplementationMethod?.Name ?? "Unknown";
        public static readonly Func<IIncomingGrainCallContext, string> NoopOldGrainMethodFormatter = x => "Noop";

        private readonly GrainMethodFormatterDelegate formatMethodName;
        private readonly IGrainProfiler profiler;
        private readonly ILogger<GrainProfilerFilter> logger;
        private readonly ConcurrentDictionary<MethodInfo, bool> shouldSkipCache = new ConcurrentDictionary<MethodInfo, bool>();

        public GrainProfilerFilter(IGrainProfiler profiler, ILogger<GrainProfilerFilter> logger, GrainMethodFormatterDelegate formatMethodName,
            Func<IIncomingGrainCallContext, string> oldFormatMethodName)
        {
            this.profiler = profiler;

            if (oldFormatMethodName != NoopOldGrainMethodFormatter)
            {
                this.formatMethodName = new GrainMethodFormatterDelegate(oldFormatMethodName);
            }
            else
            {
                this.formatMethodName = formatMethodName ?? DefaultGrainMethodFormatter;
            }

            this.logger = logger;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (!profiler.IsEnabled)
            {
                await context.Invoke();
                return;
            }

            if (ShouldSkipProfiling(context))
            {
                await context.Invoke();
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await context.Invoke();

                Track(context, stopwatch, false);
            }
            catch (Exception)
            {
                Track(context, stopwatch, true);
                throw;
            }
        }

        private void Track(IIncomingGrainCallContext context, Stopwatch stopwatch, bool isException)
        {
            try
            {
                stopwatch.Stop();

                var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

                var grainMethodName = formatMethodName(context);

                profiler.Track(elapsedMs, context.Grain.GetType(), grainMethodName, isException);
            }
            catch (Exception ex)
            {
                logger.LogError(100002, ex, "error recording results for grain");
            }
        }

        private bool ShouldSkipProfiling(IIncomingGrainCallContext context)
        {
            var grainMethod = context.ImplementationMethod;

            if (grainMethod == null)
            {
                return false;
            }

            if (!shouldSkipCache.TryGetValue(grainMethod, out var shouldSkip))
            {
                try
                {
                    var grainType = context.Grain.GetType();

                    shouldSkip =
                        grainType.GetCustomAttribute<NoProfilingAttribute>() != null ||
                        grainMethod.GetCustomAttribute<NoProfilingAttribute>() != null;
                }
                catch (Exception ex)
                {
                    logger.LogError(100003, ex, "error reading NoProfilingAttribute attribute for grain");

                    shouldSkip = false;
                }

                shouldSkipCache.TryAdd(grainMethod, shouldSkip);
            }

            return shouldSkip;
        }
    }
}
