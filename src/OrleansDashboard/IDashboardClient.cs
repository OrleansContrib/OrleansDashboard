using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;

namespace OrleansDashboard
{
    public interface IDashboardClient
    {
        Task<Immutable<DashboardCounters>> DashboardCounters();
        Task<Immutable<Dictionary<string, GrainTraceEntry>>> ClusterStats();
        Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize);
        Task<Immutable<SiloRuntimeStatistics[]>> HistoricalStats(string siloGrain);
        Task<Immutable<Dictionary<string, string>>> SiloProperties(string siloGrain);
        Task<Immutable<Dictionary<string, GrainTraceEntry>>> SiloStats(string siloAddress);
        Task<Immutable<StatCounter[]>> GetCounters(string siloAddress);
        Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GrainStats(string grainName);
        Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods();
    }
}