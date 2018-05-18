using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.History;

namespace OrleansDashboard
{
    public interface IDashboardGrain : IGrainWithIntegerKey
    {
        Task Init();

        Task<DashboardCounters> GetCounters();

        Task SubmitTracing(string siloAddress, Immutable<SiloGrainTraceEntry[]> grainCallTime);

        Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GetGrainTracing(string grain);

        Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetClusterTracing();

        Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetSiloTracing(string address);

        Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods();
    }
}