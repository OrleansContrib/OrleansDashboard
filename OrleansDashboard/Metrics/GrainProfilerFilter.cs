using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace OrleansDashboard.Metrics
{
    public class GrainProfilerFilter : IIncomingGrainCallFilter
    {
        public delegate string GrainMethodFormatterDelegate(IIncomingGrainCallContext callContext);

        public static readonly GrainMethodFormatterDelegate DefaultGrainMethodFormatter = c => c.ImplementationMethod?.Name ?? "Unknown";

        private readonly GrainMethodFormatterDelegate formatMethodName;
        private readonly Timer timer;
        private readonly IGrainProfiler profiler;
        private readonly ILogger<GrainProfilerFilter> logger;

        public GrainProfilerFilter(IGrainProfiler profiler, ILogger<GrainProfilerFilter> logger, GrainMethodFormatterDelegate formatMethodName)
        {
            this.profiler = profiler;
            this.logger = logger;
            this.formatMethodName = formatMethodName ?? DefaultGrainMethodFormatter;
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
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

        private static bool ShouldSkipProfiling(IIncomingGrainCallContext context)
        {
            return
                context.Grain.GetType().GetCustomAttribute<NoProfilingAttribute>() != null ||
                context.ImplementationMethod?.GetCustomAttribute<NoProfilingAttribute>() != null;
        }
    }
}
