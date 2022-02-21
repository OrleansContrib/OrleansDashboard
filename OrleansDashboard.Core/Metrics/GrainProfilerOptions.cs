using System;

namespace OrleansDashboard.Metrics
{
    public sealed class GrainProfilerOptions
    {
        public bool TraceAlways { get; set; }

        public TimeSpan DeactivationTime { get; set; } = TimeSpan.FromMinutes(1);
    }
}
