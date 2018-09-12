using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Client.Dispatchers;
using OrleansDashboard.Client.Interfaces;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard.Client
{
    public class DashboardSiloClient
    {
        private readonly IGrainFactory _client;
        private readonly IExternalDispatcher _dispatcher;

        public DashboardSiloClient(IGrainFactory client, IExternalDispatcher dispatcher)
        {
            _client = client;
            _dispatcher = dispatcher;
        }

        public async Task<DashboardCounters> DashboardCounters()
        {
            var grain = _client.GetGrain<IDashboardGrain>(0);
            var result = await _dispatcher.DispatchAsync(grain.GetCounters).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<Dictionary<string, GrainTraceEntry>> ClusterStats()
        {
            var grain = _client.GetGrain<IDashboardGrain>(0);
            var result = await _dispatcher.DispatchAsync(grain.GetClusterTracing).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<ReminderResponse> Reminders(int pageSize)
        {
            try
            {
                var grain = _client.GetGrain<IDashboardRemindersGrain>(0);
                var result = await _dispatcher.DispatchAsync(() => grain.GetReminders(1, pageSize))
                    .ConfigureAwait(false);

                return result.Value;
            }
            catch
            {
                // if reminders are not configured, the call to the grain will fail
                return new ReminderResponse {Reminders = new ReminderInfo[0], Count = 0};
            }
        }

        public async Task<ReminderResponse> Reminders(int page, int pageSize)
        {
            try
            {
                var grain = _client.GetGrain<IDashboardRemindersGrain>(0);
                var result = await _dispatcher.DispatchAsync(() => grain.GetReminders(page, pageSize))
                    .ConfigureAwait(false);

                return result.Value;
            }
            catch
            {
                // if reminders are not configured, the call to the grain will fail
                return new ReminderResponse {Reminders = new ReminderInfo[0], Count = 0};
            }
        }

        public async Task<SiloRuntimeStatistics[]> HistoricalStats(string key)
        {
            var grain = _client.GetGrain<ISiloGrain>(key);
            var result = await _dispatcher.DispatchAsync(grain.GetRuntimeStatistics).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<Dictionary<string, string>> SiloProperties(string key)
        {
            var grain = _client.GetGrain<ISiloGrain>(key);
            var result = await _dispatcher.DispatchAsync(grain.GetExtendedProperties).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<Dictionary<string, GrainTraceEntry>> SiloStats(string key)
        {
            var grain = _client.GetGrain<IDashboardGrain>(0);
            var result = await _dispatcher.DispatchAsync(() => grain.GetSiloTracing(key)).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<StatCounter[]> SiloCounters(string key)
        {
            var grain = _client.GetGrain<ISiloGrain>(key);
            var result = await _dispatcher.DispatchAsync(grain.GetCounters).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<Dictionary<string, Dictionary<string, GrainTraceEntry>>> GrainStats(string key)
        {
            var grain = _client.GetGrain<IDashboardGrain>(0);
            var result = await _dispatcher.DispatchAsync(() => grain.GetGrainTracing(key)).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<Dictionary<string, GrainMethodAggregate[]>> TopGrainMethods()
        {
            var grain = _client.GetGrain<IDashboardGrain>(0);
            var result = await _dispatcher.DispatchAsync(() => grain.TopGrainMethods()).ConfigureAwait(false);

            return result.Value;
        }
    }
}