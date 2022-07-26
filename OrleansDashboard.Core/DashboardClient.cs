﻿using System.Collections.Generic;
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

        public async Task<Immutable<SiloRuntimeStatistics[]>> HistoricalStats(string siloAddress)
        {
            return await Silo(siloAddress).GetRuntimeStatistics().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, string>>> SiloProperties(string siloAddress)
        {
            return await Silo(siloAddress).GetExtendedProperties().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> SiloStats(string siloAddress)
        {
            return await dashboardGrain.GetSiloTracing(siloAddress).ConfigureAwait(false);
        }

        public async Task<Immutable<StatCounter[]>> GetCounters(string siloAddress)
        {
            return await Silo(siloAddress).GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GrainStats(string grainName)
        {
            return await dashboardGrain.GetGrainTracing(grainName).ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods(int take)
        {
            return await dashboardGrain.TopGrainMethods(take).ConfigureAwait(false);
        }

        private ISiloGrain Silo(string siloAddress)
        {
            return grainFactory.GetGrain<ISiloGrain>(siloAddress);
        }

        public async Task<Immutable<string>> GetGrainState(string id, string grainType)
        {
            return await dashboardGrain.GetGrainState(id, grainType);
        }

        public async Task<Immutable<IEnumerable<string>>> GetGrainTypes()
        {
            return await dashboardGrain.GetGrainTypes();
        }
    }
}
