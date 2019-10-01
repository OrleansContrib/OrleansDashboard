using System;

namespace OrleansDashboard.Model
{
    [Serializable]
    public class SiloGrainTraceEntry
    {
        public string Grain { get; set; }
        public string Method { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
    }
}