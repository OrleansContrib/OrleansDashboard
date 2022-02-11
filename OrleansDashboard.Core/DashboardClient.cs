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
            return await dashboardGrain.GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> ClusterStats()
        {
            return await dashboardGrain.GetClusterTracing().ConfigureAwait(false);
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            return await remindersGrain.GetReminders(pageNumber, pageSize).ConfigureAwait(false);
        }

        public async Task<Immutable<SiloRuntimeStatistics[]>> HistoricalStats(string siloGrain)
        {
            var grain = grainFactory.GetGrain<ISiloGrain>(siloGrain);
            return await grain.GetRuntimeStatistics().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, string>>> SiloProperties(string siloGrain)
        {
            var grain = grainFactory.GetGrain<ISiloGrain>(siloGrain);
            return await grain.GetExtendedProperties().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> SiloStats(string siloAddress)
        {
            return await dashboardGrain.GetSiloTracing(siloAddress).ConfigureAwait(false);
        }

        public async Task<Immutable<StatCounter[]>> GetCounters(string siloAddress)
        {
            var grain = grainFactory.GetGrain<ISiloGrain>(siloAddress);
            return await grain.GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GrainStats(string grainName)
        {
            return await dashboardGrain.GetGrainTracing(grainName).ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods()
        {
            return await dashboardGrain.TopGrainMethods().ConfigureAwait(false);
        }

        public async Task<string> GetInteractionsGraph()
        {
            return await dashboardGrain.GetInteractionsGraph().ConfigureAwait(false);
        }
    }
}
