namespace OrleansDashboard.Model
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
}