using System;

namespace OrleansDashboard.Model
{
    [Serializable]
    public class GrainInteractionInfoEntry
    {
        public string Grain { get; set; }
        public string TargetGrain { get; set; }
        public string Method { get; set; }
        public uint Count { get; set; } = 1;
        
        public string Key => Grain + ":" + (TargetGrain ?? string.Empty) + ":" + Method;
    }
}