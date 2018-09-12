using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OrleansDashboard
{
    internal class AggregatedGrainTotals
    {
        public double TotalAwaitTime { get; set; }
        public long TotalCalls { get; set; }
        public long TotalExceptions { get; set; }
    }
}