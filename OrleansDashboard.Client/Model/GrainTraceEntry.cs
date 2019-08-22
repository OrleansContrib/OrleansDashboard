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

        public int CompareTo(object obj)
        {
            return obj is GrainTraceEntry entry ? CompareTo(entry) : -1;
        }

        public int CompareTo(GrainTraceEntry other)
        {
            var compared = string.Compare(Grain, other.Grain, StringComparison.OrdinalIgnoreCase);

            if (compared == 0)
            {
                return string.Compare(Method, other.Method, StringComparison.OrdinalIgnoreCase);
            }

            return compared;
        }

        public override bool Equals(object obj)
        {
            return obj is GrainTraceEntry entry && Equals(entry);
        }

        public bool Equals(GrainTraceEntry other)
        {
            return other != null && string.Equals(Grain, other.Grain, StringComparison.OrdinalIgnoreCase) && string.Equals(Method, other.Method, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (Grain?.GetHashCode() ?? 0) ^ (113 * Method?.GetHashCode() ?? 0);
        }

        public override string ToString() => $"{Grain}.{Method}";
    }
}