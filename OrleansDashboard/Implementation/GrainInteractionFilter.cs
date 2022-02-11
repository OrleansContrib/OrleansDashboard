using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Metrics;
using OrleansDashboard.Model;

namespace OrleansDashboard.Implementation
{
    public sealed class GrainInteractionFilter : IIncomingGrainCallFilter, IOutgoingGrainCallFilter
    {
        private readonly IGrainFactory grainFactory;
        private IInteractionProfiler grainInteractionProfiler;

        public GrainInteractionFilter(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                var call = GetGrainMethod(context);
                TrackBeginInvoke(call.Grain, call.Method);
                await context.Invoke();
            }
            finally
            {
                await TrackEndInvoke();
            }
        }

        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            try
            {
                var call = GetGrainMethod(context);
                TrackBeginInvoke(call.Grain, call.Method);
                await context.Invoke();
            }
            finally
            {
                await TrackEndInvoke();
            }
        }

        private Stack<GrainInteractionInfoEntry> GetCallStack()
        {
            return RequestContext.Get(nameof(GrainInteractionFilter)) as Stack<GrainInteractionInfoEntry> ?? new Stack<GrainInteractionInfoEntry>();
        }

        private void SaveCallStack(Stack<GrainInteractionInfoEntry> stack)
        {
            if (stack.Count == 0)
            {
                RequestContext.Remove(nameof(GrainInteractionFilter));
            }
            else
            {
                RequestContext.Set(nameof(GrainInteractionFilter), stack);
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

            grainInteractionProfiler ??= grainFactory.GetGrain<IInteractionProfiler>(0);
            await grainInteractionProfiler.Track(info);
            SaveCallStack(stack);
        }

        private (string Grain, string Method) GetGrainMethod(IIncomingGrainCallContext context)
        {
            return (context.InterfaceMethod.ReflectedType.Name, context.InterfaceMethod.Name);
        }

        private (string Grain, string Method) GetGrainMethod(IOutgoingGrainCallContext context)
        {
            return (context.InterfaceMethod.ReflectedType.Name, context.InterfaceMethod.Name);
        }
    }
}