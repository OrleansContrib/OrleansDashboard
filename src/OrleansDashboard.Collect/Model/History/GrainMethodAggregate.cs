namespace OrleansDashboard.Model.History
{
    public struct GrainMethodAggregate
    {
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
        public long NumberOfSamples { get; set; }
    }
}