using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Model;

namespace OrleansDashboard.Implementation
{
    public class DashboardTelemetryExporter : BaseExporter<Metric>
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

        private readonly Dictionary<string, Value<double>> metrics = new Dictionary<string, Value<double>>();
        private readonly ILocalSiloDetails localSiloDetails;
        private readonly IGrainFactory grainFactory;
        private readonly ILogger<DashboardTelemetryExporter> logger;
        private string siloAddress;

        public DashboardTelemetryExporter(ILocalSiloDetails localSiloDetails, IGrainFactory grainFactory, ILogger<DashboardTelemetryExporter> logger)
        {
            this.localSiloDetails = localSiloDetails;
            this.grainFactory = grainFactory;
            this.logger = logger;
        }

        public override ExportResult Export(in Batch<Metric> batch)
        {
            if (siloAddress == null)
            {
                siloAddress = localSiloDetails.SiloAddress.ToParsableString();
            }

            var grain = grainFactory.GetGrain<ISiloGrain>(siloAddress);

            CollectMetricsFromBatch(batch);

            if (metrics.Count == 0)
            {
                return ExportResult.Success;
            }

            var counters = new StatCounter[metrics.Count];
            var countersIndex = 0;

            foreach (var (key, value) in metrics)
            {
                // In case new values have been added to metrics in another thread. It will be pushed the next time then.
                if (countersIndex == counters.Length)
                {
                    break;
                }

                counters[countersIndex] =
                    new StatCounter(
                        key, 
                        value.Current.ToString(CultureInfo.InvariantCulture),
                        ComputeDelta(value));

                countersIndex++;
            }

            grain.ReportCounters(counters.AsImmutable());
            return ExportResult.Success;
        }

        private void CollectMetricsFromBatch(in Batch<Metric> batch)
        {
            foreach (var metric in batch)
            {
                switch (metric.MetricType)
                {
                    case MetricType.LongSum:
                        CollectMetric(metric, p => p.GetSumLong());
                        break;
                    case MetricType.DoubleSum:
                        CollectMetric(metric, p => p.GetSumDouble());
                        break;
                    case MetricType.LongGauge:
                        CollectMetric(metric, p => p.GetGaugeLastValueLong());
                        break;
                    case MetricType.DoubleGauge:
                        CollectMetric(metric, p => p.GetGaugeLastValueDouble());
                        break;
                    case MetricType.Histogram:
                        CollectMetric(metric, p => p.GetHistogramSum());
                        break;
                    default:
                        logger.LogWarning("Ignoring unknown metric type {MetricType}", metric.MetricType);
                        break;
                }
            }
        }

        private void CollectMetric(Metric metric, Func<MetricPoint, double> getValue)
        {
            foreach (var point in metric.GetMetricPoints())
            {
                var value = getValue(point);
                if (!metrics.ContainsKey(metric.Name))
                    metrics[metric.Name] = new Value<double>(0);
                metrics[metric.Name] = metrics[metric.Name].Update(value);
            }
        }

        private static string ComputeDelta(Value<double> metric)
        {
            var delta = metric.Current - metric.Last;

            return delta.ToString(CultureInfo.InvariantCulture);
        }
    }
}