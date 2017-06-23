using System;
using System.Collections.Generic;
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
        public DateTime Period { get; set; }
        public string SiloAddress { get; set; }
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
        public string GrainAndMethod => $"{this.Grain}.{this.Method}";
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
        public SiloDetails[] Hosts { get; set; }
        public SimpleGrainStatisticCounter[] SimpleGrainStats { get; set; }
        public int TotalActivationCount { get; set; }
        public Queue<int> TotalActivationCountHistory { get; set; }
    }

    internal class GrainSiloKey
    {
        public GrainSiloKey(string grainName, string siloAddress)
        {
            GrainName = grainName;
            SiloAddress = siloAddress;
        }

        public string GrainName { get; }

        public string SiloAddress { get; }

        protected bool Equals(GrainSiloKey other)
        {
            return string.Equals(GrainName, other.GrainName)
                && string.Equals(SiloAddress, other.SiloAddress);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GrainSiloKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (GrainName.GetHashCode() * 397) ^ SiloAddress.GetHashCode();
            }
        }

        public static bool operator ==(GrainSiloKey left, GrainSiloKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GrainSiloKey left, GrainSiloKey right)
        {
            return !Equals(left, right);
        }
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
    }
}