using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrleansDashboard.Metrics
{
    public static class GrainProfilerExtensions
    {
        public static void Track<T>(this IGrainProfiler profiler, double elapsedMs, [CallerMemberName] string methodName = null, bool failed = false)
        {
            profiler.Track(elapsedMs, typeof(T), methodName, failed);
        }

        public static async Task TrackAsync<T>(this IGrainProfiler profiler, Func<Task> handler, [CallerMemberName] string methodName = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await handler();

                stopwatch.Stop();

                profiler.Track(stopwatch.Elapsed.TotalMilliseconds, typeof(T), methodName);
            }
            catch (Exception)
            {
                stopwatch.Stop();

                profiler.Track(stopwatch.Elapsed.TotalMilliseconds, typeof(T), methodName, true);
                throw;
            }
        }
    }
}
