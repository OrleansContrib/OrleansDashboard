using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Client.Model;
using OrleansDashboard.Client.Model.History;

namespace OrleansDashboard.Client
{
    public class DashboardClient : IDashboardClient
    {
        private readonly IGrainFactory grainFactory;

        public DashboardClient(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<Immutable<DashboardCounters>> DashboardCounters()
        {
            var grain = grainFactory.GetGrain<IDashboardGrain>(0);
            return await grain.GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> ClusterStats()
        {
            var grain = grainFactory.GetGrain<IDashboardGrain>(0);
            return await grain.GetClusterTracing().ConfigureAwait(false);
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            var grain = grainFactory.GetGrain<IDashboardRemindersGrain>(0);
            return await grain.GetReminders(pageNumber, pageSize).ConfigureAwait(false);
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
            var grain = grainFactory.GetGrain<IDashboardGrain>(0);
            return await grain.GetSiloTracing(siloAddress).ConfigureAwait(false);
        }

        public async Task<Immutable<StatCounter[]>> GetCounters(string siloAddress)
        {
            var grain = grainFactory.GetGrain<ISiloGrain>(siloAddress);
            return await grain.GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GrainStats(string grainName)
        {
            var grain = grainFactory.GetGrain<IDashboardGrain>(0);
            return await grain.GetGrainTracing(grainName).ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods()
        {
            var grain = grainFactory.GetGrain<IDashboardGrain>(0);
            return await grain.TopGrainMethods().ConfigureAwait(false);
        }
    }
}
