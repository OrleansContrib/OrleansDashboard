using System;
using System.Collections.Generic;
using System.Linq;
using OrleansDashboard.Client.Model;
using OrleansDashboard.Client.Model.History;

namespace OrleansDashboard.Metrics.History
{
    public class TraceHistory : ITraceHistory
    {
        const int HistoryDurationInSeconds = 100;
        private readonly LinkedList<GrainTraceEntry> history = new LinkedList<GrainTraceEntry>();
        private readonly HashSet<GrainTraceEntry> allMethods = new HashSet<GrainTraceEntry>(GrainTraceEqualityComparer.ByGrainAndMethodAndSilo);

        public Dictionary<string, GrainTraceEntry> QueryAll()
        {
            return GetTracings(history);
        }

        public Dictionary<string, GrainTraceEntry> QuerySilo(string siloAddress)
        {
            return GetTracings(history.Where(x => string.Equals(x.SiloAddress, siloAddress, StringComparison.OrdinalIgnoreCase)));
        }

        private static Dictionary<string, GrainTraceEntry> GetTracings(IEnumerable<GrainTraceEntry> traces)
        {
            var result = new Dictionary<string, GrainTraceEntry>();

            var entries = traces.ToLookup(x => x.PeriodKey);

            var time = GetRetirementWindow(DateTime.UtcNow);

            for (var i = 0; i < HistoryDurationInSeconds; i++)
            {
                time = time.AddSeconds(1);

                var periodKey = time.ToPeriodString();

                var entry = new GrainTraceEntry
                {
                    Period = time,
                    PeriodKey = periodKey
                };

                foreach (var trace in entries[periodKey])
                {
                    entry.Count += trace.Count;
                    entry.ElapsedTime += trace.ElapsedTime;
                    entry.ExceptionCount += trace.ExceptionCount;
                }

                result[periodKey] = entry;
            }

            return result;
        }


        public void Add(DateTime now, string siloAddress, SiloGrainTraceEntry[] grainTrace)
        {
            var retirementWindow = GetRetirementWindow(now);

            var current = history.First;

            while (current != null)
            {
                var next = current.Next;

                if (current.Value.Period < retirementWindow)
                {
                    history.Remove(current);
                }

                current = next;
            }

            var periodKey = now.ToPeriodString();

            var added = new HashSet<GrainTraceEntry>(GrainTraceEqualityComparer.ByGrainAndMethod);

            foreach (var entry in grainTrace)
            {
                var newEntry = new GrainTraceEntry
                {
                    Count = entry.Count,
                    ElapsedTime = entry.ElapsedTime,
                    ExceptionCount = entry.ExceptionCount,
                    Grain = entry.Grain,
                    Method = entry.Method,
                    Period = now,
                    SiloAddress = siloAddress,
                    PeriodKey = periodKey
                };

                if (!allMethods.Contains(newEntry))
                {
                    allMethods.Add(new GrainTraceEntry
                    {
                        Count = 0,
                        ElapsedTime = 0,
                        Grain = newEntry.Grain,
                        Method = newEntry.Method,
                        Period = now,
                        SiloAddress = siloAddress,
                        PeriodKey = periodKey
                    });
                }

                if (added.Add(newEntry))
                {
                    history.AddLast(newEntry);
                }
            }

            // fill in any previously captured methods which aren't in this reporting window
            foreach (var siloMethod in allMethods)
            {
                if (string.Equals(siloMethod.SiloAddress, siloAddress, StringComparison.OrdinalIgnoreCase) && added.Add(siloMethod))
                {
                    history.AddLast(siloMethod);
                }
            }
        }

        private static DateTime GetRetirementWindow(DateTime now)
        {
            return now.AddSeconds(-HistoryDurationInSeconds);
        }

        public Dictionary<string, Dictionary<string, GrainTraceEntry>> QueryGrain(string grain)
        {
            const string SEPARATOR = ".";

            var results = new Dictionary<string, Dictionary<string, GrainTraceEntry>>();

            foreach (var historicValue in history.Where(x => x.Grain == grain))
            {
                var grainMethodKey = string.Join(SEPARATOR, grain, historicValue.Method);

                if (!results.TryGetValue(grainMethodKey, out var grainResults))
                {
                    results[grainMethodKey] = grainResults = new Dictionary<string, GrainTraceEntry>();
                }

                var key = historicValue.PeriodKey;

                if (!grainResults.TryGetValue(grainMethodKey, out var value))
                {
                    grainResults[key] = value = new GrainTraceEntry
                    {
                        Grain = historicValue.Grain,
                        Method = historicValue.Method,
                        Period = historicValue.Period
                    };
                }

                value.Count += historicValue.Count;
                value.ElapsedTime += historicValue.ElapsedTime;
                value.ExceptionCount += historicValue.ExceptionCount;
            }
            
            return results;

        }

        public IEnumerable<TraceAggregate> GroupByGrainAndSilo()
        {
            return history.GroupBy(x => (x.Grain, x.SiloAddress)).Select(group => {
                var result = new TraceAggregate
                {
                    SiloAddress = group.Key.SiloAddress,
                    Grain = group.Key.Grain
                };
                foreach (var record in group)
                {
                    result.Count += record.Count;
                    result.ExceptionCount += record.ExceptionCount;
                    result.ElapsedTime += record.ElapsedTime;
                }
                return result;
            });
        }

        public IEnumerable<GrainMethodAggregate> AggregateByGrainMethod()
        {
            return history
                .GroupBy(x => (x.Grain, x.Method))
                .Select(x => {
                    var aggregate = new GrainMethodAggregate 
                    {
                        Grain = x.Key.Grain,
                        Method = x.Key.Method,
                        NumberOfSamples = HistoryDurationInSeconds // this will give the wrong answer during the first 100 seconds
                    };
                    foreach (var value in x)
                    {
                        aggregate.Count += value.Count;
                        aggregate.ElapsedTime += value.ElapsedTime;
                        aggregate.ExceptionCount += value.ExceptionCount;
                    }
                    return aggregate;
                });
        }
    }
}