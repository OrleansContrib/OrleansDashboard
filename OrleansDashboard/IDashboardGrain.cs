using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace OrleansDashboard
{
    public interface IDashboardGrain : IGrainWithIntegerKey
    {
        Task Init();

        Task<DashboardCounters> GetCounters();

        Task SubmitTracing(string siloIdentity, GrainTraceEntry[] grainCallTime);

        Task<Dictionary<string, Dictionary<string, GrainTraceEntry>>> GetGrainTracing(string grain);

        Task<Dictionary<string, GrainTraceEntry>> GetClusterTracing();

        Task<Dictionary<string, GrainTraceEntry>> GetSiloTracing(string address);
    }
}