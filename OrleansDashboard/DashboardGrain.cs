using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    [Reentrant]
    [PreferLocalPlacement]
    public class DashboardGrain : Grain, IDashboardGrain
    {
        private static readonly TimeSpan DefaultTimerInterval = TimeSpan.FromSeconds(1);
        private readonly DashboardCounters counters = new DashboardCounters();
        private readonly List<GrainTraceEntry> history = new List<GrainTraceEntry>();
        private readonly DashboardOptions options;
        private readonly ISiloDetailsProvider siloDetailsProvider;
        private DateTime startTime = DateTime.UtcNow;

        public DashboardGrain(IOptions<DashboardOptions> options, ISiloDetailsProvider siloDetailsProvider)
        {
            this.options = options.Value;
            this.siloDetailsProvider = siloDetailsProvider;
        }
        
        private async Task Callback(object _)
        {
            var metricsGrain = GrainFactory.GetGrain<IManagementGrain>(0);
            var activationCountTask = metricsGrain.GetTotalActivationCount();
            var simpleGrainStatsTask = metricsGrain.GetSimpleGrainStatistics();
            var siloDetailsTask = siloDetailsProvider.GetSiloDetails();

            await Task.WhenAll(activationCountTask,  simpleGrainStatsTask, siloDetailsTask);

            RecalculateCounters(activationCountTask.Result, siloDetailsTask.Result, simpleGrainStatsTask.Result);
        }

        internal void RecalculateCounters(int activationCount, SiloDetails[] hosts,
            IList<SimpleGrainStatistic> simpleGrainStatistics)
        {
            counters.TotalActivationCount = activationCount;

            counters.TotalActiveHostCount = hosts.Count(x => x.SiloStatus == SiloStatus.Active);
            counters.TotalActivationCountHistory.Enqueue(activationCount);
            counters.TotalActiveHostCountHistory.Enqueue(counters.TotalActiveHostCount);

            while (counters.TotalActivationCountHistory.Count > Dashboard.HistoryLength)
            {
                counters.TotalActivationCountHistory.Dequeue();
            }
            while (counters.TotalActiveHostCountHistory.Count > Dashboard.HistoryLength)
            {
                counters.TotalActiveHostCountHistory.Dequeue();
            }

            // TODO - whatever max elapsed time
            var elapsedTime = Math.Min((DateTime.UtcNow - startTime).TotalSeconds, 100);

            counters.Hosts = hosts;

            var aggregatedTotals = history.ToLookup(x => new GrainSiloKey(x.Grain, x.SiloAddress));

            counters.SimpleGrainStats = simpleGrainStatistics.Select(x =>
            {
                var grainName = TypeFormatter.Parse(x.GrainType);
                var siloAddress = x.SiloAddress.ToParsableString();

                var result = new SimpleGrainStatisticCounter
                {
                    ActivationCount = x.ActivationCount,
                    GrainType = grainName,
                    SiloAddress = siloAddress,
                    TotalSeconds = elapsedTime
                };

                foreach (var item in aggregatedTotals[new GrainSiloKey(grainName, siloAddress)])
                {
                    result.TotalAwaitTime += item.ElapsedTime;
                    result.TotalCalls += item.Count;
                    result.TotalExceptions += item.ExceptionCount;
                }

                return result;
            }).ToArray();
        }

        public override Task OnActivateAsync()
        {
            var timerInterval = DefaultTimerInterval;

            if (options.CounterUpdateIntervalMs > 0)
            {
                timerInterval = TimeSpan.FromHours(options.CounterUpdateIntervalMs);
            }

            if (timerInterval < DefaultTimerInterval)
            {
                timerInterval = DefaultTimerInterval;
            }

            try
            {
                RegisterTimer(Callback, null, timerInterval, timerInterval);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Not running in Orleans runtime");
            }

            startTime = DateTime.UtcNow;

            return base.OnActivateAsync();
        }

        public Task<DashboardCounters> GetCounters()
        {
            return Task.FromResult(counters);
        }

        public Task<Dictionary<string, Dictionary<string, GrainTraceEntry>>> GetGrainTracing(string grain)
        {
            var results = new Dictionary<string, Dictionary<string, GrainTraceEntry>>();

            foreach (var historicValue in history.Where(x => x.Grain == grain))
            {
                var grainMethodKey = $"{grain}.{historicValue.Method}";

                if (!results.TryGetValue(grainMethodKey, out var grainResults))
                {
                    results[grainMethodKey] = grainResults = new Dictionary<string, GrainTraceEntry>();
                }

                var key = historicValue.Period.ToPeriodString();

                if (!grainResults.TryGetValue(grainMethodKey, out var value))
                {
                    grainResults[key] = value = new GrainTraceEntry
                    {
                        Grain = historicValue.Grain,
                        Method = historicValue.Method,
                        Period = historicValue.Period
                    };
                }

                value.Count += historicValue.Count;
                value.ElapsedTime += historicValue.ElapsedTime;
                value.ExceptionCount += historicValue.ExceptionCount;
            }

            return Task.FromResult(results);
        }

        public Task<Dictionary<string, GrainTraceEntry>> GetClusterTracing()
        {
            var results = new Dictionary<string, GrainTraceEntry>();

            foreach (var historicValue in history)
            {
                var key = historicValue.Period.ToPeriodString();

                if (!results.TryGetValue(key, out var value))
                {
                    results[key] = value = new GrainTraceEntry
                    {
                        Period = historicValue.Period
                    };
                }

                value.Count += historicValue.Count;
                value.ElapsedTime += historicValue.ElapsedTime;
                value.ExceptionCount += historicValue.ExceptionCount;
            }

            return Task.FromResult(results);
        }

        public Task<Dictionary<string, GrainTraceEntry>> GetSiloTracing(string address)
        {
            var results = new Dictionary<string, GrainTraceEntry>();

            foreach (var historicValue in history.Where(x => x.SiloAddress == address))
            {
                var key = historicValue.Period.ToPeriodString();

                if (!results.TryGetValue(key, out var value))
                {
                    results[key] = value = new GrainTraceEntry
                    {
                        Period = historicValue.Period
                    };
                }

                value.Count += historicValue.Count;
                value.ElapsedTime += historicValue.ElapsedTime;
                value.ExceptionCount += historicValue.ExceptionCount;
            }

            return Task.FromResult(results);
        }

        public Task Init()
        {
            // just used to activate the grain
            return Task.CompletedTask;
        }

        public Task SubmitTracing(string siloIdentity, GrainTraceEntry[] grainTrace)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in grainTrace)
            {
                // sync clocks
                entry.Period = now;
            }

            // fill in any previously captured methods which aren't in this reporting window
            var allGrainTrace = new List<GrainTraceEntry>(grainTrace);
            var values = history.Where(x => x.SiloAddress == siloIdentity).GroupBy(x => x.GrainAndMethod).Select(x => x.First());
            foreach (var value in values)
            {
                if (!grainTrace.Any(x => x.GrainAndMethod == value.GrainAndMethod))
                {
                    allGrainTrace.Add(new GrainTraceEntry
                    {
                        Count = 0,
                        ElapsedTime = 0,
                        Grain = value.Grain,
                        Method = value.Method,
                        Period = now,
                        SiloAddress = siloIdentity
                    });
                }
            }

            var retirementWindow = DateTime.UtcNow.AddSeconds(-100);
            history.AddRange(allGrainTrace);
            history.RemoveAll(x => x.Period < retirementWindow);

            return Task.CompletedTask;
        }
    }
}