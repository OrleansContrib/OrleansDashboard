﻿namespace OrleansDashboard.Client.Model
{
    public sealed class ReminderResponse
    {
        public int Count { get; set; }

        public ReminderInfo[] Reminders { get; set; }
    }
}