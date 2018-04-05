using Orleans;
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

        Task ReportCounters(StatCounter[] stats);

        Task<Dictionary<string,string>> GetExtendedProperties();

        Task<SiloRuntimeStatistics[]> GetRuntimeStatistics();

        Task<StatCounter[]> GetCounters();
    }
}
