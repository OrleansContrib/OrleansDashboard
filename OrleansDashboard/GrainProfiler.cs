using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class GrainProfiler : IDisposable
    {
        public TaskScheduler TaskScheduler { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }
        object sync = new object();
        string siloAddress;
        public Logger Logger { get; private set; }

        readonly Func<MethodInfo, InvokeMethodRequest, IGrain, string> formatMethodName = 
            (targetMethod, _, __) => targetMethod?.Name ?? "Unknown";

        public GrainProfiler(TaskScheduler taskScheduler, IProviderRuntime providerRuntime)
        {
            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;
            this.Logger = this.ProviderRuntime.GetLogger("GrainProfiler");

            // check if custom method name formatter is registered
            var formatter = providerRuntime.ServiceProvider.GetService<Func<MethodInfo, InvokeMethodRequest, IGrain, string>>();
            if (formatter != null)
                formatMethodName = formatter;

            // register interceptor, wrapping any previously set interceptor
            this.innerInterceptor = providerRuntime.GetInvokeInterceptor();
            providerRuntime.SetInvokeInterceptor(this.InvokeInterceptor);
            siloAddress = providerRuntime.SiloIdentity.ToSiloAddress();

            // register timer to report every second
            timer = new Timer(this.ProcessStats, providerRuntime, 1 * 1000, 1 * 1000);

        }

        async Task<object> Dispatch(Func<Task<object>> func)
        {
            return await Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.TaskScheduler);
        }


        // capture stats
        async Task<object> InvokeInterceptor(MethodInfo targetMethod, InvokeMethodRequest request, IGrain grain, IGrainMethodInvoker invoker)
        {
            var grainName = grain.GetType().FullName;
            var stopwatch = Stopwatch.StartNew();

            // invoke grain
            object result = null;
            var isException = false;

            try
            {
                if (this.innerInterceptor != null)
                {
                    result = await this.innerInterceptor(targetMethod, request, grain, invoker).ConfigureAwait(false);
                }
                else
                {
                    result = await invoker.Invoke(grain, request).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {

                try
                {
                    stopwatch.Stop();

                    var elapsedMs = (double)stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

                    var key = string.Format("{0}.{1}", grainName, formatMethodName(targetMethod, request, grain));

                    grainTrace.AddOrUpdate(key, _ =>
                    {
                        return new SiloGrainTraceEntry
                        {
                            Count = 1,
                            ExceptionCount = (isException ? 1 : 0),
                            ElapsedTime = elapsedMs,
                            Grain = grainName ,
                            Method = formatMethodName(targetMethod, request, grain),
                        };
                    },
                    (_, last) =>
                    {
                        last.Count += 1;
                        last.ElapsedTime += elapsedMs;
                        if (isException) last.ExceptionCount += 1;
                        return last;
                    });
                }
                catch (Exception ex)
                {
                    this.Logger.Error(100002, "error recording results for grain", ex);
                }
            }

            return result;
        }

        Timer timer = null;
        ConcurrentDictionary<string, SiloGrainTraceEntry> grainTrace = new ConcurrentDictionary<string, SiloGrainTraceEntry>();

        // publish stats to a grain
        void ProcessStats(object state)
        {
            var providerRuntime = state as IProviderRuntime;
            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            // flush the dictionary
            SiloGrainTraceEntry[] data;
            lock (sync)
            {
                data = this.grainTrace.Values.ToArray();
                this.grainTrace.Clear();
            }

            foreach (var item in data)
            {
                item.Grain = TypeFormatter.Parse(item.Grain);
            }

            try
            {
                Dispatch(async () =>
                {
                    await dashboardGrain.SubmitTracing(siloAddress, data).ConfigureAwait(false);
                    return null;
                }).ContinueWith(result => {

                    if (null != result.Exception)
                    {
                        this.Logger.Log(100001, Severity.Warning, "Exception thrown sending tracing to dashboard grain", new object[0], result.Exception);
                    }
                });
                
            }
            catch (Exception ex)
            {
                this.Logger.Log(100001, Severity.Warning, "Exception thrown sending tracing to dashboard grain", new object[0], ex);
            }
            
        }

        public void Dispose()
        {
            if (null == timer) return;
            timer.Dispose();
        }

        InvokeInterceptor innerInterceptor = null;

    }
}
