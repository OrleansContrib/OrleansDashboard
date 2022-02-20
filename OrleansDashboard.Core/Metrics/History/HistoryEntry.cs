﻿using System;

namespace OrleansDashboard.Metrics.History
{
    public record struct HistoryEntry
    {
        public DateTime Period { get; set; }

        public long PeriodNumber { get; set; }

        public long Count { get; set; }

        public long ExceptionCount { get; set; }

        public double ElapsedTime { get; set; }
    }
}
