using Orleans;

namespace OrleansDashboard.Model
{
    [GenerateSerializer]
    [Immutable]
    public readonly struct StatCounter
    {
        [Id(0)]
        public readonly string Name;

        [Id(1)]
        public readonly string Value;

        [Id(2)]
        public readonly string Delta;

        public StatCounter(string name, string value, string delta) : this()
        {
            Name = name;
            Value = value;
            Delta = delta;
        }
    }
}