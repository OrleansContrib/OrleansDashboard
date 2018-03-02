using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;
using Orleans.Runtime;

namespace Dashboard
{
    public class GrainCallFilter : IIncomingGrainCallFilter, IOutgoingGrainCallFilter
    {
        private string siloAddress;
        private ConcurrentDictionary<string, GrainTraceEntry> traces;
        private Timer timer = null;
        object sync = new object();

        private TaskScheduler orleansTS;
        private IProviderRuntime providerRuntime;

        readonly Func<MethodInfo, IAddressable, string> formatMethodName =
            (targetMethod, __) => targetMethod?.Name ?? "Unknown";

        public GrainCallFilter(IProviderRuntime providerRuntime)
        {
            traces = new ConcurrentDictionary<string, GrainTraceEntry>();

            siloAddress = providerRuntime.SiloIdentity;

            this.providerRuntime = providerRuntime;

            orleansTS = TaskScheduler.Current;

            timer = new Timer(this.ProcessStats, new
            {
                ts = orleansTS,
                pr = providerRuntime
            }, 1 * 1000, 1 * 1000);
        }

        async Task IIncomingGrainCallFilter.Invoke(IGrainCallContext context)
        {
            await ProcessInvoke(context, "Incoming");
        }

        async Task IOutgoingGrainCallFilter.Invoke(IGrainCallContext context)
        {
            await ProcessInvoke(context, "Outgoing");
        }

        private async Task ProcessInvoke(IGrainCallContext context, string direction)
        {
            var stopwatch = Stopwatch.StartNew();
            var grainName = context.Grain.GetType().FullName;

            var isException = false;

            try
            {
                await context.Invoke();
            }
            catch (Exception e)
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

                    var key = string.Format("{0}.{1}", grainName, formatMethodName(context.Method, context.Grain));

                    traces.AddOrUpdate(key, _ => new GrainTraceEntry
                    {
                        Count = 1,
                        ExceptionCount = (isException ? 1 : 0),
                        SiloAddress = siloAddress,
                        ElapsedTime = elapsedMs,
                        Grain = grainName,
                        Method = formatMethodName(context.Method, context.Grain),
                        Period = DateTime.UtcNow
                    },
                        (_, last) =>
                        {
                            last.Count += 1;
                            last.ElapsedTime += elapsedMs;
                            if (isException) last.ExceptionCount += 1;
                            return last;
                        });
                }
                catch (Exception c)
                {

                }
            }
        }

        //async Task<object> Dispatch(Func<Task<object>> func)
        //{
        //    return await Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.TaskScheduler);
        //}

        void ProcessStats(dynamic state)
        {
            var providerRuntime = state.pr as IProviderRuntime;
            var dashboardGrain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var orleansTaskScheduler = state.ts as TaskScheduler;

            // flush the dictionary
            GrainTraceEntry[] data;
            lock (sync)
            {
                data = this.traces.Values.ToArray();
                this.traces.Clear();
            }

            foreach (var item in data)
            {
                item.Grain = TypeFormatter.Parse(item.Grain);
            }

            Task.Factory.StartNew(() =>
            {
                dashboardGrain.SubmitTracing(siloAddress, data).ConfigureAwait(false);
            }, CancellationToken.None, TaskCreationOptions.None, scheduler: orleansTaskScheduler).ConfigureAwait(false);


            //try
            //{
            //    Dispatch(async () =>
            //    {
            //        await dashboardGrain.SubmitTracing(siloAddress, data).ConfigureAwait(false);
            //        return null;
            //    }).ContinueWith(result =>
            //    {

            //        if (null != result.Exception)
            //        {
            //            this.Logger.Log(100001, Severity.Warning, "Exception thrown sending tracing to dashboard grain", new object[0], result.Exception);
            //        }
            //    });

            //}
            //catch (Exception ex)
            //{
            //    //this.Logger.Log(100001, Severity.Warning, "Exception thrown sending tracing to dashboard grain", new object[0], ex);
            //}

        }
    }
}
