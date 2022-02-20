﻿using System;
using System.Collections.Generic;
using System.Linq;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;

namespace OrleansDashboard.Metrics.History
{
    public sealed class TraceHistoryV2 : ITraceHistory
    {
        private readonly Dictionary<HistoryKey, RingBuffer<HistoryEntry>> history = new Dictionary<HistoryKey, RingBuffer<HistoryEntry>>(100);
        private readonly int capacity;

        public TraceHistoryV2(int capacity = 100)
        {
            this.capacity = capacity;
        }

        public Dictionary<string, Dictionary<string, GrainTraceEntry>> QueryGrain(string grain)
        {
            var results = new Dictionary<string, Dictionary<string, GrainTraceEntry>>();

            foreach (var group in history.Where(x => x.Key.Grain == grain).GroupBy(x => (x.Key.Grain, x.Key.Method)))
            {
                var grainMethodKey = $"{group.Key.Grain}.{group.Key.Method}";

                results[grainMethodKey] = GetTracings(group);
            }

            return results;
        }

        public Dictionary<string, GrainTraceEntry> QueryAll()
        {
            return GetTracings(history.ToList());
        }

        public Dictionary<string, GrainTraceEntry> QuerySilo(string siloAddress)
        {
            return GetTracings(history.Where(x => string.Equals(x.Key.SiloAddress, siloAddress, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        private Dictionary<string, GrainTraceEntry> GetTracings(IEnumerable<KeyValuePair<HistoryKey, RingBuffer<HistoryEntry>>> traces)
        {
            var time = GetRetirementWindow(DateTime.UtcNow);

            var periodStart = time.ToPeriodNumber();

            var aggregations = new GrainTraceEntry[capacity];

            foreach (var traceList in traces)
            {
                var bufferList = traceList.Value;
                var bufferCount = bufferList.Count;

                for (var j = 0; j < bufferCount; j++)
                {
                    var trace = bufferList[j];

                    var resultIndex = trace.PeriodNumber - periodStart;

                    if (resultIndex >= 0 && resultIndex < capacity)
                    {
                        var entry = aggregations[resultIndex] ?? new GrainTraceEntry();

                        entry.Count += trace.Count;
                        entry.ElapsedTime += trace.ElapsedTime;
                        entry.ExceptionCount += trace.ExceptionCount;

                        aggregations[resultIndex] = entry;
                    }
                }
            }

            var result = new Dictionary<string, GrainTraceEntry>(capacity);

            for (var i = 0; i < capacity; i++)
            {
                time = time.AddSeconds(1);

                var periodKey = time.ToPeriodString();

                var entry = aggregations[i] ??= new GrainTraceEntry();

                entry.Period = time;
                entry.PeriodKey = periodKey;

                result[periodKey] = entry;
            }

            return result;
        }


        public void Add(DateTime now, string siloAddress, SiloGrainTraceEntry[] grainTrace)
        {
            var periodNumber = now.ToPeriodNumber();

            foreach (var trace in grainTrace)
            {
                var key = new HistoryKey(siloAddress, trace.Grain, trace.Method);

                if (!history.TryGetValue(key, out var historyBuffer))
                {
                    historyBuffer = new RingBuffer<HistoryEntry>(capacity);
                    history[key] = historyBuffer;
                }

                historyBuffer.Add(new HistoryEntry
                {
                    Period = now,
                    PeriodNumber = periodNumber,
                    ExceptionCount = trace.ExceptionCount,
                    ElapsedTime = trace.ElapsedTime,
                    Count = trace.Count
                });
            }
        }

        private DateTime GetRetirementWindow(DateTime now)
        {
            return now.AddSeconds(-capacity);
        }

        public IEnumerable<TraceAggregate> GroupByGrainAndSilo()
        {
            return history.GroupBy(x => (x.Key.Grain, x.Key.SiloAddress)).Select(group =>
            {
                var result = new TraceAggregate
                {
                    SiloAddress = group.Key.SiloAddress,
                    Grain = group.Key.Grain,
                };

                foreach (var traceList in group)
                {
                    var bufferList = traceList.Value;
                    var bufferCount = bufferList.Count;

                    for (var i = 0; i < bufferCount; i++)
                    {
                        var record = bufferList[i];

                        result.Count += record.Count;
                        result.ExceptionCount += record.ExceptionCount;
                        result.ElapsedTime += record.ElapsedTime;
                    }
                }

                return result;
            });
        }

        public IEnumerable<GrainMethodAggregate> AggregateByGrainMethod()
        {
            return history.GroupBy(x => (x.Key.Grain, x.Key.Method)).Select(group =>
            {
                var result = new GrainMethodAggregate
                {
                    Grain = group.Key.Grain,
                    Method = group.Key.Method,
                    NumberOfSamples = capacity
                };

                foreach (var traceList in group)
                {
                    var bufferList = traceList.Value;
                    var bufferCount = bufferList.Count;

                    for (var i = 0; i < bufferCount; i++)
                    {
                        var record = bufferList[i];

                        result.Count += record.Count;
                        result.ExceptionCount += record.ExceptionCount;
                        result.ElapsedTime += record.ElapsedTime;
                    }
                }

                return result;
            });
        }
    }
}