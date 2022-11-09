using System;
using System.Collections.Generic;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;

namespace OrleansDashboard.Metrics.History
{
    public interface ITraceHistory
    {
        void Add(DateTime time, string siloAddress, SiloGrainTraceEntry[] grainTrace);

        Dictionary<string, GrainTraceEntry> QueryAll();

        Dictionary<string, GrainTraceEntry> QuerySilo(string siloAddress);

        Dictionary<string, Dictionary<string, GrainTraceEntry>> QueryGrain(string grain);

        IEnumerable<TraceAggregate> GroupByGrainAndSilo();

        IEnumerable<GrainMethodAggregate> AggregateByGrainMethod();
    }
}
