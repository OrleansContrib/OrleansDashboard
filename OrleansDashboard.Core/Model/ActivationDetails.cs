using Orleans;
using Orleans.Runtime;
using System;

namespace OrleansDashboard.Model
{
    [GenerateSerializer]
    public sealed class ActivationDetails
    {
        [Id(0)]
        public GrainId GrainId { get; set; }
        [Id(1)]
        public Guid? GuidId { get; set; }
        [Id(2)]
        public long? IntId { get; set; }
    }
}