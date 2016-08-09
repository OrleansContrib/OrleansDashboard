using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class GrainProfiler
    {
        public TaskScheduler TaskScheduler { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }

        public GrainProfiler(TaskScheduler taskScheduler, IProviderRuntime providerRuntime)
        {
            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;

            // register interceptor
            providerRuntime.SetInvokeInterceptor(this.InvokeInterceptor);

            // register timer
            timer = new Timer(this.ProcessStats, providerRuntime, 10 * 1000, 10 * 1000);
        }

        Task<object> Dispatch(Func<Task<object>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.TaskScheduler).Result;
        }


        // capture stats
        async Task<object> InvokeInterceptor(MethodInfo targetMethod, InvokeMethodRequest request, IGrain grain, IGrainMethodInvoker invoker)
        {
            // round down to nearest 10 seconds to group results
            var grainName = grain.GetType().FullName;
            // round to the nearest 10 seconds
            var period = DateTime.UtcNow.ToString(@"yyyy/MM/dd HH:mm:ss").Substring(0, 18) + "0";

            var stopwatch = Stopwatch.StartNew();

            // invoke grain
            var result = await invoker.Invoke(grain, request);

            stopwatch.Stop();

            var elapsedMs = (double)stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

            var grainTrace = this.GrainTraceHistory.GetOrAdd(period, _ => new ConcurrentDictionary<string, GrainTraceEntry>());
            var key = $"{grainName}.{targetMethod?.Name ?? "?"}";

            grainTrace.AddOrUpdate(key, _ => {
                return new GrainTraceEntry
                {
                    Count = 1,
                    ElapsedTime = elapsedMs,
                    Grain = grainName,
                    Method = targetMethod?.Name ?? "?",
                    Period = DateTime.Parse(period)
                };
            },
            (_, last) => {
                last.Count += 1;
                last.ElapsedTime += elapsedMs;
                return last;
            });

            return result;
        }

        Timer timer = null;
        ConcurrentDictionary<string, ConcurrentDictionary<string, GrainTraceEntry>> GrainTraceHistory = new ConcurrentDictionary<string, ConcurrentDictionary<string, GrainTraceEntry>>();

        // publish stats to a grain
        void ProcessStats(object state)
        {
            var providerRuntime = state as IProviderRuntime;
            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var retirementWindow = DateTime.UtcNow.AddSeconds(-10 * 100);

            ConcurrentDictionary<string, GrainTraceEntry> _;
            foreach (var key in this.GrainTraceHistory.Keys.ToArray())
            {
                if (DateTime.Parse(key) >= retirementWindow) continue;
                this.GrainTraceHistory.TryRemove(key, out _);
            }
            
            Dispatch(async () =>
            {
                await dashboardGrain.SubmitTracing(providerRuntime.SiloIdentity.ToSiloAddress() , this.GrainTraceHistory.OrderBy(x => x.Key).Select(x => x.Value as IDictionary<string, GrainTraceEntry>).ToArray());
                return null;
            }).Wait();
            
        }

    }
}
