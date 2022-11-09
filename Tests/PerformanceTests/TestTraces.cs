using OrleansDashboard.Model;
using System;

namespace PerformanceTests
{
    internal sealed record TestTraces(DateTime Time, string Silo, SiloGrainTraceEntry[] Traces)
    {
    }
}
