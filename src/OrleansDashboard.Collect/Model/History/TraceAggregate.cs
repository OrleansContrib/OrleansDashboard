namespace OrleansDashboard.Model.History
{
    public struct TraceAggregate
    {
        public string SiloAddress { get; set; }
        public string Grain { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
    }
}