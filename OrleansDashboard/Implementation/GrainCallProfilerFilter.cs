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

namespace OrleansDashboard.Metrics
{
    public class GrainCallProfilerFilter : IIncomingGrainCallFilter, IOutgoingGrainCallFilter
    {
        private readonly IGrainProfiler profiler;
        private readonly ILogger<GrainCallProfilerFilter> logger;
        private readonly ConcurrentDictionary<MethodInfo, bool> shouldSkipCache = new ConcurrentDictionary<MethodInfo, bool>();
        

        
        
        public GrainCallProfilerFilter(IGrainProfiler profiler, ILogger<GrainCallProfilerFilter> logger)
        {
            this.profiler = profiler;
            this.logger = logger;
        }
        

        private bool ShouldSkipProfiling(IGrainCallContext context)
        {
            var grainMethod = context.InterfaceMethod;

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
        
        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (ShouldSkipProfiling(context))
            {
                await context.Invoke();
                return;
            }
            
            if (IsDebgu(context.InterfaceMethod.Name))
            {
                //in
                var call = GetGrainMethod(context);
                TrackBeginInvoke(call.Grain, call.Method);
            }
            
            try
            {
                await context.Invoke();
            }
            finally
            {
                if (IsDebgu(context.InterfaceMethod.Name))
                {
                    //in
                    TrackEndInvoke();
                }

               
            }
        }
        
        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            if (ShouldSkipProfiling(context))
            {
                await context.Invoke();
                return;
            }

          
            if (IsDebgu(context.InterfaceMethod.Name))
            {
                //out
                var call = GetGrainMethod(context);
                TrackBeginInvoke(call.Grain, call.Method);
            }
            
            try
            {
                await context.Invoke();
            }
            finally
            {
                if (IsDebgu(context.InterfaceMethod.Name))
                {
                    //out
                    TrackEndInvoke();
                }
            }
        }

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
        
        private bool IsDebgu(string name)
        {
            if (name == "CallSecondInteractionTestGrain" ||
                name == "CallThirdInteractionTestGrain" ||
                name == "ITestGrain" ||
                name == "CallFirstInteractionTestGrain")
                return true;

            return false;
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

        private void TrackEndInvoke()
        {
            var stack = GetCallStack();
            var info = stack.Pop();

            //grainInteractionProfiler ??= grainFactory.GetGrain<IInteractionProfiler>(0);
            //await grainInteractionProfiler.Track(info);
            SaveCallStack(stack);

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

[Serializable]
public class GrainInteractionInfoEntry
{
    public string Grain { get; set; }
    public string TargetGrain { get; set; }
    public string Method { get; set; }
    public uint Count { get; set; } = 1;
        
    public string Key => Grain + ":" + (TargetGrain ?? string.Empty) + ":" + Method;
}

