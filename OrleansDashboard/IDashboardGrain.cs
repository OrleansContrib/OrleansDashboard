using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansDashboard
{

    public class DashboardGrainSettings
    {
        public int GrainSampleFrequncyMs { get; set; }
    }

    public interface IDashboardGrain : IGrain, IGrainWithIntegerKey
    {
        Task Init(DashboardGrainSettings settings);

        Task<DashboardCounters> GetCounters();

        Task SubmitTracing(string siloIdentity, SiloGrainTraceEntry[] grainCallTime);

        Task<Dictionary<string, Dictionary<string, GrainTraceEntry>>> GetGrainTracing(string grain);

        Task<Dictionary<string, GrainTraceEntry>> GetClusterTracing();

        Task<Dictionary<string, GrainTraceEntry>> GetSiloTracing(string address);
    }
}
