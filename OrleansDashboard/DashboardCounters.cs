using System;
using System.Collections.Generic;
using Orleans.Runtime;
using System.Collections.Concurrent;
using System.Linq;

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
        public object TotalSeconds { get; internal set; }
    }

    [Serializable]
    public class GrainTraceEntry
    {
        public DateTime Period { get; set; }
        public string SiloAddress { get; set; }
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public double ElapsedTime { get; set; }
        public string GrainAndMethod => $"{this.Grain}.{this.Method}";
    }

    public class DashboardCounters
    {
        public DashboardCounters()
        {
            Hosts = new Dictionary<string, string>();
            SimpleGrainStats = new SimpleGrainStatisticCounter[0];
            TotalActivationCountHistory = new Queue<int>();
            TotalActiveHostCountHistory = new Queue<int>();
            foreach (var x in Enumerable.Range(1, Dashboard.HistoryLength))
            {
                this.TotalActivationCountHistory.Enqueue(0);
                this.TotalActiveHostCountHistory.Enqueue(0);
            }
        }

        public int TotalActiveHostCount { get; set; }
        public Queue<int> TotalActiveHostCountHistory { get; set; }
        public Dictionary<string, string> Hosts { get; set; }
        public SimpleGrainStatisticCounter[] SimpleGrainStats { get; set; }
        public int TotalActivationCount { get; set; }
        public Queue<int> TotalActivationCountHistory { get; set; }



    }
}
