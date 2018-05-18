using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Orleans.Runtime;

namespace OrleansDashboard
{
    public class SimpleGrainStatisticCounter
    {
        public int ActivationCount { get; set; }
        public string GrainType { get; set; }
        public string SiloAddress { get; set; }
        public double TotalAwaitTime { get; set; }
        public long TotalCalls { get; set; }
        public double CallsPerSecond { get; set; }
        public object TotalSeconds { get; set; }
        public long TotalExceptions { get; set; }
    }

    [Serializable]
    public class GrainTraceEntry
    {
        public string PeriodKey {get;set;}
        public DateTime Period { get; set; }
        public string SiloAddress { get; set; }
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
    }

    [Serializable]
    public class SiloGrainTraceEntry
    {
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
    }

    public class SiloDetails
    {
        public int FaultZone { get; set; }
        public string HostName { get; set; }
        public string IAmAliveTime { get; set; }
        public int ProxyPort { get; set; }
        public string RoleName { get; set; }
        public string SiloAddress { get; set; }
        public string SiloName { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public int UpdateZone { get; set; }
        public SiloStatus SiloStatus { get; set; }
    }

    public class DashboardCounters
    {
        public DashboardCounters()
        {
            Hosts = new SiloDetails[0];
            SimpleGrainStats = new SimpleGrainStatisticCounter[0];
            TotalActivationCountHistory = ImmutableQueue<int>.Empty;
            TotalActiveHostCountHistory = ImmutableQueue<int>.Empty;
            foreach (var x in Enumerable.Range(1, Dashboard.HistoryLength))
            {
                TotalActivationCountHistory = TotalActivationCountHistory.Enqueue(0);
                TotalActiveHostCountHistory = TotalActiveHostCountHistory.Enqueue(0);
            }
        }

        public int TotalActiveHostCount { get; set; }
        public ImmutableQueue<int> TotalActiveHostCountHistory { get; set; }
        public SiloDetails[] Hosts { get; set; }
        public SimpleGrainStatisticCounter[] SimpleGrainStats { get; set; }
        public int TotalActivationCount { get; set; }
        public ImmutableQueue<int> TotalActivationCountHistory { get; set; }
    }

    internal class AggregatedGrainTotals
    {
        public double TotalAwaitTime { get; set; }
        public long TotalCalls { get; set; }
        public long TotalExceptions { get; set; }
    }

    public class ReminderInfo
    {
        public string GrainReference { get; set; }
        public string Name { get; set; }
        public DateTime StartAt { get; set; }
        public TimeSpan Period { get; set; }
        public string PrimaryKey { get; set; }
    }
}