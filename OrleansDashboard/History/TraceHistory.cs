using System;
using System.Collections.Generic;
using System.Linq;
using OrleansDashboard.Client.Model;
using OrleansDashboard.Client.Model.History;

namespace OrleansDashboard.History
{
    public class TraceHistory : ITraceHistory
    {

        const int HistoryLength = 100;
        private readonly List<GrainTraceEntry> history = new List<GrainTraceEntry>();

        public Dictionary<string, GrainTraceEntry> QueryAll()
        {
            return GetTracings(history);
        }

        public Dictionary<string, GrainTraceEntry> QuerySilo(string siloAddress)
        {
            return GetTracings(history.Where(x => string.Equals(x.SiloAddress, siloAddress, StringComparison.OrdinalIgnoreCase)));
        }

        private Dictionary<string, GrainTraceEntry> GetTracings(IEnumerable<GrainTraceEntry> traces)
        {
            var results = new Dictionary<string, GrainTraceEntry>();

            foreach (var group in traces.GroupBy(x => x.PeriodKey))
            {
                var entry = new GrainTraceEntry{
                    Period = group.First().Period
                };

                foreach (var item in group)
                {
                    entry.Count += item.Count;
                    entry.ElapsedTime += item.ElapsedTime;
                    entry.ExceptionCount += item.ExceptionCount;
                }
                results.Add(group.Key, entry);
            }

            return results;
        }


        public void Add(DateTime now, string siloAddress, SiloGrainTraceEntry[] grainTrace)
        {
            var allGrainTrace = new List<GrainTraceEntry>(grainTrace.Length);

            var retirementWindow = now.AddSeconds(-HistoryLength);
            history.RemoveAll(x => x.Period <= retirementWindow);

            var periodKey = now.ToPeriodString();
            foreach (var entry in grainTrace)
            {
                var grainTraceEntry = new GrainTraceEntry
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

                allGrainTrace.Add(grainTraceEntry);
            }

            // fill in any previously captured methods which aren't in this reporting window
            var values = history.Where(x => string.Equals(x.SiloAddress, siloAddress, StringComparison.OrdinalIgnoreCase)).GroupBy(x => (x.Grain, x.Method)).Select(x => x.First());
            foreach (var value in values)
            {
                if (!grainTrace.Any(x => 
                    string.Equals(x.Grain, value.Grain, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Method, value.Method, StringComparison.OrdinalIgnoreCase)))
                {
                    allGrainTrace.Add(new GrainTraceEntry
                    {
                        Count = 0,
                        ElapsedTime = 0,
                        Grain = value.Grain,
                        Method = value.Method,
                        Period = now,
                        SiloAddress = siloAddress,
                        PeriodKey = periodKey
                    });
                }
            }

            history.AddRange(allGrainTrace);
        }

        public Dictionary<string, Dictionary<string, GrainTraceEntry>> QueryGrain(string grain)
        {
            var results = new Dictionary<string, Dictionary<string, GrainTraceEntry>>();

            foreach (var historicValue in history.Where(x => x.Grain == grain))
            {
                const string SEPARATOR = ".";
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
            return this.history.GroupBy(x => (x.Grain, x.SiloAddress)).Select(group => {
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
                        NumberOfSamples = HistoryLength // this will give the wrong answer during the first 100 seconds
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