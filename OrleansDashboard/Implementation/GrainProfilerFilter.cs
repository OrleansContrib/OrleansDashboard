using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Implementation;
using OrleansDashboard.Model;

namespace OrleansDashboard.Metrics
{
    public class GrainProfilerFilter : IIncomingGrainCallFilter, IOutgoingGrainCallFilter
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
            this.logger = logger;

            if (oldFormatMethodName != NoopOldGrainMethodFormatter)
            {
                this.formatMethodName = new GrainMethodFormatterDelegate(oldFormatMethodName);
            }
            else
            {
                this.formatMethodName = formatMethodName ?? DefaultGrainMethodFormatter;
            }
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            var info = GetGrainTypeAndMethodInfo(context);
            if (ShouldSkipProfiling(info.GrainType, info.GrainMethodInfo))
            {
                await context.Invoke();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var call = GetGrainMethod(context);
            TrackBeginInvoke(call.Grain, call.Method);

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
            finally
            {
                await TrackEndInvoke();
            }
        }
        
        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            var info = GetGrainTypeAndMethodInfo(context);
            if (ShouldSkipProfiling(info.GrainType, info.GrainMethodInfo))
            {
                await context.Invoke();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var call = GetGrainMethod(context);
            TrackBeginInvoke(call.Grain, call.Method);

            try
            {
                await context.Invoke();

                //Track(context, stopwatch, false);
            }
            catch (Exception)
            {
                //Track(context, stopwatch, true);
                throw;
            }
            finally
            {
                await TrackEndInvoke();
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

        private void TrackBeginInvoke(string grain, string method)
        {
            var stack = GetCallStack();
            if (stack.TryPeek(out var info))
            {
                info.TargetGrain = grain;
            }

            stack.Push(new GrainInteractionInfoEntry
            {
                Grain = grain,
                Method = method
            });
            SaveCallStack(stack);
        }

        private async Task TrackEndInvoke()
        {
            var stack = GetCallStack();
            var info = stack.Pop();
            SaveCallStack(stack);
        }
        
        private bool ShouldSkipProfiling(Type GrainType, MethodInfo GrainMethodInfo)
        {
            if (GrainMethodInfo == null)
            {
                return false;
            }

            if (!shouldSkipCache.TryGetValue(GrainMethodInfo, out var shouldSkip))
            {
                try
                {
                    var grainType = GrainType;

                    shouldSkip =
                        grainType.GetCustomAttribute<NoProfilingAttribute>() != null ||
                        GrainMethodInfo.GetCustomAttribute<NoProfilingAttribute>() != null;
                }
                catch (Exception ex)
                {
                    logger.LogError(100003, ex, "error reading NoProfilingAttribute attribute for grain");

                    shouldSkip = false;
                }

                shouldSkipCache.TryAdd(GrainMethodInfo, shouldSkip);
            }

            return shouldSkip;
        }
        
        
        
        /////
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Stack<GrainInteractionInfoEntry> GetCallStack()
        {
            return RequestContext.Get(nameof(GrainProfilerFilter)) as Stack<GrainInteractionInfoEntry> ?? new Stack<GrainInteractionInfoEntry>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveCallStack(Stack<GrainInteractionInfoEntry> stack)
        {
            if (stack.Count == 0)
            {
                RequestContext.Remove(nameof(GrainProfilerFilter));
            }
            else
            {
                RequestContext.Set(nameof(GrainProfilerFilter), stack);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (Type GrainType, MethodInfo GrainMethodInfo) GetGrainTypeAndMethodInfo(IIncomingGrainCallContext context)
        {
            return (context.Grain.GetType(), context.ImplementationMethod);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (Type GrainType, MethodInfo GrainMethodInfo) GetGrainTypeAndMethodInfo(IOutgoingGrainCallContext context)
        {
            return (context.Grain.GetType(), context.InterfaceMethod);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (string Grain, string Method) GetGrainMethod(IIncomingGrainCallContext context)
        {
            return (context.InterfaceMethod.ReflectedType.Name, context.InterfaceMethod.Name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (string Grain, string Method) GetGrainMethod(IOutgoingGrainCallContext context)
        {
            return (context.InterfaceMethod.ReflectedType.Name, context.InterfaceMethod.Name);
        }
    }
}
