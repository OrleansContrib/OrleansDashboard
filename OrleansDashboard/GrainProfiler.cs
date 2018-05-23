using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Concurrency;

namespace OrleansDashboard
{
    public class GrainProfiler : IIncomingGrainCallFilter
    {
        private static readonly Func<IIncomingGrainCallContext, string> DefaultFormatter = c => c.ImplementationMethod?.Name ?? "Unknown";

        private readonly Func<IIncomingGrainCallContext, string> formatMethodName;
        private readonly Timer timer;
        private readonly ILogger<GrainProfiler> logger;
        private readonly ILocalSiloDetails localSiloDetails;
        private readonly IExternalDispatcher dispatcher;
        private readonly IGrainFactory grainFactory;
        private ConcurrentDictionary<string, SiloGrainTraceEntry> grainTrace = new ConcurrentDictionary<string, SiloGrainTraceEntry>();
        private string siloAddress;
        private IDashboardGrain dashboardGrain;

        public GrainProfiler(
            ILogger<GrainProfiler> logger,
            ILocalSiloDetails localSiloDetails,
            IExternalDispatcher dispatcher,
            IServiceProvider services,
            IGrainFactory grainFactory)
        {
            this.dispatcher = dispatcher;
            this.logger = logger;
            this.localSiloDetails = localSiloDetails;
            this.grainFactory = grainFactory;

            formatMethodName = services.GetService<Func<IIncomingGrainCallContext, string>>() ?? DefaultFormatter;

            // register timer to report every second
            timer = new Timer(ProcessStats, null, 1 * 1000, 1 * 1000);
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (siloAddress == null)
            {
                siloAddress = localSiloDetails.SiloAddress.ToParsableString();
            }

            var stopwatch = Stopwatch.StartNew();
            
            var isException = false;

            try
            {
                await context.Invoke();
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

                    var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                    var grainName = context.Grain.GetType().FullName;
                    var methodName = formatMethodName(context);

                    var key = string.Format("{0}.{1}", grainName, methodName);

                    var exceptionCount = (isException ? 1 : 0);

                    grainTrace.AddOrUpdate(key, _ => 
                        new SiloGrainTraceEntry
                        {
                            Count = 1,
                            ExceptionCount = exceptionCount,
                            ElapsedTime = elapsedMs,
                            Grain = grainName ,
                            Method = methodName
                        },
                    (_, last) =>
                    {
                        last.Count += 1;
                        last.ElapsedTime += elapsedMs;

                        if (isException)
                        {
                            last.ExceptionCount += exceptionCount;
                        }

                        return last;
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(100002, "error recording results for grain", ex);
                }
            }
        }

        private void ProcessStats(object state)
        {
            if (dispatcher.CanDispatch())
            {
                var currentTrace = Interlocked.Exchange(ref grainTrace, new ConcurrentDictionary<string, SiloGrainTraceEntry>());

                var items = currentTrace.Values.ToArray();

                foreach (var item in items)
                {
                    item.Grain = TypeFormatter.Parse(item.Grain);
                }

                try
                {
                    dispatcher.DispatchAsync(() =>
                    {
                        this.dashboardGrain = this.dashboardGrain ?? grainFactory.GetGrain<IDashboardGrain>(0);

                        return dashboardGrain.SubmitTracing(siloAddress, items.AsImmutable());
                    }).Ignore();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(100001, "Exception thrown sending tracing to dashboard grain", ex);
                }
            }
        }
    }
}
