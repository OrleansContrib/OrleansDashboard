using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.Model;

namespace OrleansDashboard.Metrics.Grains
{
    [Reentrant]
    public class InteractionProfilerGrain : Grain, IInteractionProfiler
    {
        private const int DefaultTimerIntervalMs = 1000; // 1 second
        private readonly Dictionary<string, Dictionary<string, GrainInteractionInfoEntry>> interaction = new();
        private readonly DashboardOptions options;
        private IDisposable timer;

        public InteractionProfilerGrain(IOptions<DashboardOptions> options)
        {
            this.options = options.Value;
        }

        public Task Track(GrainInteractionInfoEntry entry)
        {
            if (interaction.TryGetValue(entry.Grain, out var existing))
            {
                if (existing.TryGetValue(entry.Key, out var existingEntry))
                {
                    existingEntry.Count++;
                }
                else
                {
                    existing[entry.Key] = entry;
                }
            }
            else
            {
                interaction.Add(entry.Grain, new Dictionary<string, GrainInteractionInfoEntry>
                {
                    [entry.Key] = entry
                });
            }

            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            var updateInterval = TimeSpan.FromMilliseconds(Math.Max(options.CounterUpdateIntervalMs, DefaultTimerIntervalMs));

            try
            {
                timer = RegisterTimer(x => CollectStatistics((bool)x), true, updateInterval, updateInterval);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Not running in Orleans runtime");
            }

            await base.OnActivateAsync();
        }

        private async Task CollectStatistics(bool canDeactivate)
        {
            var dashboardGrain = GrainFactory.GetGrain<IDashboardGrain>(0);
            try
            {
                await dashboardGrain.SubmitGrainInteraction(BuildGraph());
            }
            catch (Exception)
            {
                // we can't get the silo stats, it's probably dead, so kill the grain
                if (canDeactivate)
                {
                    timer?.Dispose();
                    timer = null;

                    DeactivateOnIdle();
                }
            }
        }

        private string BuildGraph()
        {
            var content = string.Join("\n    ", interaction.Values.SelectMany(s => s)
                /*.Where(w=>!string.IsNullOrEmpty(w.To))*/
                .Select(s => $"{s.Value.Grain} -> {s.Value.TargetGrain ?? s.Value.Grain+"_self"} [ label = \"{s.Value.Method}\", color=\"0.650 0.700 0.700\" ];"));

            var colors = string.Join("\n", interaction.Values.SelectMany(s => s)
                .Select(s => s.Value.Grain)
                .Distinct()
                .Select(s => $"{s} [color=\"0.628 0.227 1.000\"];"));
            
            var graphCode = @$"
digraph finite_state_machine {{
rankdir=LR;
ratio = fill;
node [style=filled];
    {content}

{colors}
}}";
            return graphCode;
        }
    }
}