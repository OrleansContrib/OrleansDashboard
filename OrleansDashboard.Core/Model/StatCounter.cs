namespace OrleansDashboard.Model
{
    public readonly struct StatCounter
    {
        public readonly string Name;

        public readonly string Value;

        public readonly string Delta;

        public StatCounter(string name, string value, string delta) : this()
        {
            Name = name;
            Value = value;
            Delta = delta;
        }
    }
}