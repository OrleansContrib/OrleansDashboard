using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;

namespace OrleansDashboard
{
    public class DashboardClient : IDashboardClient
    {
        private readonly IDashboardGrain dashboardGrain;
        private readonly IDashboardRemindersGrain remindersGrain;
        private readonly IGrainFactory grainFactory;

        public DashboardClient(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
            dashboardGrain = grainFactory.GetGrain<IDashboardGrain>(0);
            remindersGrain = grainFactory.GetGrain<IDashboardRemindersGrain>(0);
        }

        public async Task<Immutable<DashboardCounters>> DashboardCounters()
        {
            return await dashboardGrain.GetCounters();
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> ClusterStats()
        {
            return await dashboardGrain.GetClusterTracing();
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            return await remindersGrain.GetReminders(pageNumber, pageSize);
        }

        public async Task<Immutable<List<SiloRuntimeStatistics>>> HistoricalStats(string siloAddress)
        {
            return await Silo(siloAddress).GetRuntimeStatistics();
        }

        public async Task<Immutable<Dictionary<string, string>>> SiloProperties(string siloAddress)
        {
            return await Silo(siloAddress).GetExtendedProperties();
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> SiloStats(string siloAddress)
        {
            return await dashboardGrain.GetSiloTracing(siloAddress);
        }

        public async Task<Immutable<StatCounter[]>> GetCounters(string siloAddress)
        {
            return await Silo(siloAddress).GetCounters();
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GrainStats(
            string grainName)
        {
            return await dashboardGrain.GetGrainTracing(grainName);
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods(int take)
        {
            return await dashboardGrain.TopGrainMethods(take);
        }

        private ISiloGrainService Silo(string siloAddress)
        {
            return grainFactory.GetGrain<ISiloGrainProxy>(siloAddress);
        }

        public async Task<Immutable<string>> GetGrainState(string id, string grainType)
        {
            return await dashboardGrain.GetGrainState(id, grainType);
        }

        public async Task<Immutable<string[]>> GetGrainTypes()
        {
            return await dashboardGrain.GetGrainTypes();
        }
    }
}