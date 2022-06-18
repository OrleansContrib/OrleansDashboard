using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;

namespace OrleansDashboard
{
    public interface IDashboardGrain : IGrainWithIntegerKey
    {
        [OneWay]
        Task Init();

        [OneWay]
        Task SubmitTracing(string siloAddress, Immutable<SiloGrainTraceEntry[]> grainCallTime);

        Task<Immutable<DashboardCounters>> GetCounters();

        Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GetGrainTracing(string grain);

        Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetClusterTracing();

        Task<Immutable<Dictionary<string, GrainTraceEntry>>> GetSiloTracing(string address);

        Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods(int take);

        Task<Immutable<string>> GetGrainState(string id, string grainType);
    }
}