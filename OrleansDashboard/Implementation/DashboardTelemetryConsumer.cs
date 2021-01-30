using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using OrleansDashboard.Model;

namespace OrleansDashboard
{
    public sealed class DashboardTelemetryConsumer : IMetricTelemetryConsumer
    {
        public readonly struct Value<T>
        {
            public readonly T Current;
            public readonly T Last;

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
        private string siloAddress;
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
            if (siloAddress == null)
            {
                siloAddress = localSiloDetails.SiloAddress.ToParsableString();
            }

            var grain = grainFactory.GetGrain<ISiloGrain>(siloAddress);

            var size = metrics.Count + timespanMetrics.Count;

            if (size == 0)
            {
                return;
            }

            var counters = new StatCounter[size];
            var countersIndex = 0;

            foreach (var (key, value) in metrics)
            {
                counters[countersIndex] =
                    new StatCounter(key, value.Current.ToString(CultureInfo.InvariantCulture),
                        ComputeDelta(value));

                countersIndex++;
            }

            foreach (var (key, value) in timespanMetrics)
            {
                counters[countersIndex] =
                    new StatCounter(key, value.Current.ToString("c", CultureInfo.InvariantCulture),
                        ComputeDelta(value));

                countersIndex++;
            }

            grain.ReportCounters(counters.AsImmutable());
        }

        private static string ComputeDelta(Value<TimeSpan> metric)
        {
            try
            {
                return (metric.Current - metric.Last).ToString("c", CultureInfo.InvariantCulture);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "";
            }
        }

        private static string ComputeDelta(Value<double> metric)
        {
            return (metric.Current - metric.Last).ToString(CultureInfo.InvariantCulture);
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
