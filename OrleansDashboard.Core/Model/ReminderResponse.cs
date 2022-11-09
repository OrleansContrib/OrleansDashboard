using Orleans;

namespace OrleansDashboard.Model
{
    [GenerateSerializer]
    public sealed class ReminderResponse
    {
        [Id(0)]
        public int Count { get; set; }

        [Id(1)]
        public ReminderInfo[] Reminders { get; set; }
    }
}