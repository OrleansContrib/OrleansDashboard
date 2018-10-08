using System;

namespace OrleansDashboard.Client.Model
{
    public class ReminderInfo
    {
        public string GrainReference { get; set; }
        public string Name { get; set; }
        public DateTime StartAt { get; set; }
        public TimeSpan Period { get; set; }
        public string PrimaryKey { get; set; }
    }
}