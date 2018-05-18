using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class StatCounter
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string Delta { get; set; }
    }

    public interface ISiloGrain : IGrainWithStringKey
    {
        Task SetVersion(string orleans, string host);

        Task ReportCounters(Immutable<StatCounter[]> stats);

        Task<Immutable<Dictionary<string,string>>> GetExtendedProperties();

        Task<Immutable<SiloRuntimeStatistics[]>> GetRuntimeStatistics();

        Task<Immutable<StatCounter[]>> GetCounters();
    }
}
