using OrleansDashboard.Client.Model;

namespace OrleansDashboard
{
    public sealed class ReminderResponse
    {
        public int Count { get; set; }

        public ReminderInfo[] Reminders { get; set; }
    }
}