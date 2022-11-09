using System;
using System.Runtime.CompilerServices;

namespace OrleansDashboard.Metrics
{
    public interface IGrainProfiler
    {
        void Track(double elapsedMs, Type grainType, [CallerMemberName] string methodName = null, bool failed = false);

        void Enable(bool enabled);

        bool IsEnabled { get; }
    }
}
