using Orleans;
using Orleans.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansDashboard
{

    public class SiloGrainSettings
    {
        public string OrleansVersion { get; set; }
        public int SiloSampleFrequncyMs { get; set; }
    }

    public interface ISiloGrain : IGrainWithStringKey
    {
        Task<SiloRuntimeStatistics[]> GetRuntimeStatistics();
        Task Init(SiloGrainSettings settings);
        Task<Dictionary<string,string>> GetExtendedProperties();
        Task ReportCounters(StatCounter[] stats);
        Task<StatCounter[]> GetCounters();
    }
}
