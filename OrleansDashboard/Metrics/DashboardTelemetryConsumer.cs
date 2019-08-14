using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using OrleansDashboard.Client;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard
{
    public sealed class DashboardTelemetryConsumer : IMetricTelemetryConsumer
    {
        public class Value<T>
        {
            public T Current;
            public T Last;

            public Value(T value)
                : this(value, value)
            {

            }

            public Value(T last, T current)
            {
                Last = last;

                Current = current;
            }

            public Value<T> Update(T newValue)
            {
                return new Value<T>(Current, newValue);
            }
        }

        private readonly ConcurrentDictionary<string, Value<double>> metrics = new ConcurrentDictionary<string, Value<double>>();
        private readonly ConcurrentDictionary<string, Value<TimeSpan>> timespanMetrics = new ConcurrentDictionary<string, Value<TimeSpan>>();
        private readonly ILocalSiloDetails localSiloDetails;
        private readonly IGrainFactory grainFactory;
        private readonly Timer timer;
        private bool isClosed;

        public DashboardTelemetryConsumer(ILocalSiloDetails localSiloDetails, IGrainFactory grainFactory)
        {
            this.localSiloDetails = localSiloDetails;
            this.grainFactory = grainFactory;

            // register timer to report every second
            timer = new Timer(x => Flush(), null, 1 * 1000, 1 * 1000);
        }

        public void DecrementMetric(string name)
        {
            DecrementMetric(name, 1);
        }

        public void IncrementMetric(string name)
        {
            IncrementMetric(name, 1);
        }

        public void DecrementMetric(string name, double value)
        {
            IncrementMetric(name, -value);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            metrics.AddOrUpdate(name, new Value<double>(value), (key, current) => current.Update(value));
        }

        public void IncrementMetric(string name, double value)
        {
            metrics.AddOrUpdate(name, new Value<double>(value), (key, current) => current.Update(current.Current + value));
        }

        public void TrackMetric(string name, TimeSpan value, IDictionary<string, string> properties = null)
        {
            timespanMetrics.AddOrUpdate(name, new Value<TimeSpan>(value), (key, current) => current.Update(value));
        }

        public void Flush()
        {
            var grain = grainFactory.GetGrain<ISiloGrain>(localSiloDetails.SiloAddress.ToParsableString());

            var countersArray = metrics.Select(metric => new StatCounter
                {
                    Name = metric.Key,
                    Value = metric.Value.Current.ToString(CultureInfo.InvariantCulture),
                    Delta = ComputeDelta(metric)
                })
                .Concat(timespanMetrics.Select(metric => new StatCounter
                {
                    Name = metric.Key,
                    Value = (metric.Value.Current).ToString("c", CultureInfo.InvariantCulture),
                    Delta = ComputeDelta(metric)
                }))
                .ToArray();

            if (countersArray.Length > 0)
            {
                grain.ReportCounters(countersArray.AsImmutable());
            }
        }

        private static string ComputeDelta(KeyValuePair<string, Value<TimeSpan>> metric)
        {
            try
            {
                return (metric.Value.Current - metric.Value.Last).ToString("c", CultureInfo.InvariantCulture);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "Invalid Values";
            }
        }

        private static string ComputeDelta(KeyValuePair<string, Value<double>> metric)
        {
            return (metric.Value.Current - metric.Value.Last).ToString(CultureInfo.InvariantCulture);
        }

        public void Close()
        {
            if (isClosed) return;

            isClosed = true;
            timer.Dispose();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
