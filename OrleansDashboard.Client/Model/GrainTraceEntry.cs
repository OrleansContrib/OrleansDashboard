using System;

namespace OrleansDashboard.Client.Model
{
    //Comparable make Distinct a little bit faster
    [Serializable]
    public class GrainTraceEntry : IComparable<GrainTraceEntry>, IComparable, IEquatable<GrainTraceEntry>
    {
        public string PeriodKey {get;set;}
        public DateTime Period { get; set; }
        public string SiloAddress { get; set; }
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }

        public int CompareTo(GrainTraceEntry other)
        {
            return string.Compare(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(GrainTraceEntry other) => CompareTo(other) == 0;

        public override string ToString() => $"{Grain}.{Method}";

        public override int GetHashCode() => ToString().GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is GrainTraceEntry entry && Equals(entry);
        }

        public int CompareTo(object obj)
        {
            return obj is GrainTraceEntry entry ? CompareTo(entry) : -1;
        }
    }
}