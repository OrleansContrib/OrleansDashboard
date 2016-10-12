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

        string siloAddress;

        public GrainProfiler(TaskScheduler taskScheduler, IProviderRuntime providerRuntime)
        {
            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;

            // register interceptor, wrapping any previously set interceptor
            this.innerInterceptor = providerRuntime.GetInvokeInterceptor();
            providerRuntime.SetInvokeInterceptor(this.InvokeInterceptor);
            siloAddress = providerRuntime.SiloIdentity.ToSiloAddress();

            // register timer to report every second
            timer = new Timer(this.ProcessStats, providerRuntime, 1 * 1000, 1 * 1000);

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
            var stopwatch = Stopwatch.StartNew();

            // invoke grain
            object result = null;
            if (this.innerInterceptor != null)
            {
                result = await this.innerInterceptor(targetMethod, request, grain, invoker);
            }
            else
            {
                result = await invoker.Invoke(grain, request);
            }

            stopwatch.Stop();

            var elapsedMs = (double)stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

            var key = $"{grainName}.{targetMethod?.Name ?? "Unknown"}";

            grainTrace.AddOrUpdate(key, _ => {
                return new GrainTraceEntry
                {
                    Count = 1,
                    SiloAddress = siloAddress,
                    ElapsedTime = elapsedMs,
                    Grain = grainName,
                    Method = targetMethod?.Name ?? "Unknown",
                    Period = DateTime.UtcNow
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
        ConcurrentDictionary<string, GrainTraceEntry> grainTrace = new ConcurrentDictionary<string, GrainTraceEntry>();

        // publish stats to a grain
        void ProcessStats(object state)
        {
            var providerRuntime = state as IProviderRuntime;
            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);
            
            // flush the dictionary
            var data = this.grainTrace.Values.ToArray();
            this.grainTrace.Clear();


            Dispatch(async () =>
            {
                await dashboardGrain.SubmitTracing(siloAddress, data);
                return null;
            }).Wait();
            
        }

        InvokeInterceptor innerInterceptor = null;

    }
}
