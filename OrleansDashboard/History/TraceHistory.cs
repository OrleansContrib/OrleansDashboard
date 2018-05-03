using System;
using System.Collections.Generic;
using System.Linq;

namespace OrleansDashboard.History
{
    public class TraceHistory : ITraceHistory
    {

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

            foreach (var historicValue in traces)
            {
                var key = historicValue.Period.ToPeriodString();

                if (!results.TryGetValue(key, out var value))
                {
                    results[key] = value = new GrainTraceEntry
                    {
                        Period = historicValue.Period
                    };
                }

                value.Count += historicValue.Count;
                value.ElapsedTime += historicValue.ElapsedTime;
                value.ExceptionCount += historicValue.ExceptionCount;
            }

            return results;
        }


        public void Add(DateTime now, string siloAddress, SiloGrainTraceEntry[] grainTrace)
        {
            var allGrainTrace = new List<GrainTraceEntry>(grainTrace.Length);

            var retirementWindow = now.AddSeconds(-100);
            history.RemoveAll(x => x.Period < retirementWindow);

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
                    SiloAddress = siloAddress
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
                        SiloAddress = siloAddress
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
                var grainMethodKey = $"{grain}.{historicValue.Method}";

                if (!results.TryGetValue(grainMethodKey, out var grainResults))
                {
                    results[grainMethodKey] = grainResults = new Dictionary<string, GrainTraceEntry>();
                }

                var key = historicValue.Period.ToPeriodString();

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
            return this.history.GroupBy(x => (x.Grain, x.SiloAddress)).Select(x => {
                return new TraceAggregate{
                    SiloAddress = x.Key.SiloAddress,
                    Grain = x.Key.Grain,
                    Count = x.Sum(y => y.Count),
                    ExceptionCount = x.Sum(y => y.ExceptionCount),
                    ElapsedTime = x.Sum(y => y.ElapsedTime)    
                };
            });
        }
    }
}