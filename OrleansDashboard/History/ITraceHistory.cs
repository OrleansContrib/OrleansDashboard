using System;
using System.Collections.Generic;

namespace OrleansDashboard.History
{

    public struct TraceAggregate
    {
        public string SiloAddress { get; set; }
        public string Grain { get; set; }
        public long Count { get; set; }
        public long ExceptionCount { get; set; }
        public double ElapsedTime { get; set; }
    }

    public interface ITraceHistory
    {

        /// <summary>
        /// Add trace information recorded at a silo 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="siloAddress"></param>
        /// <param name="grainTrace"></param>
        void Add(DateTime time, string siloAddress, SiloGrainTraceEntry[] grainTrace);

        /// <summary>
        /// Get trace information aggregated across the whole cluster in a dictionary, with a key for each second 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, GrainTraceEntry> QueryAll();

        /// <summary>
        /// Get trace information for a given silo in a dictionary, with a key for each second 
        /// </summary>
        /// <param name="siloAddress"></param>
        /// <returns></returns>
        Dictionary<string, GrainTraceEntry> QuerySilo(string siloAddress);

        /// <summary>
        /// Returns a dictionary for each GrainType.MethodName,  
        /// Each entry contains a dictionary with a key for each second in the history
        /// </summary>
        /// <param name="grain"></param>
        /// <returns></returns>
        Dictionary<string, Dictionary<string, GrainTraceEntry>> QueryGrain(string grain);

        /// <summary>
        /// aggregates across the whole history grouped by silo address and grain
        /// </summary>
        /// <returns></returns>
        IEnumerable<TraceAggregate> GroupByGrainAndSilo();

    }

}