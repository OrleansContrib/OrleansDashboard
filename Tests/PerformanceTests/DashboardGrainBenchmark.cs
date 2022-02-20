using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using OrleansDashboard.Model;
using OrleansDashboard.Metrics.History;
using System.Collections;

namespace PerformanceTests
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class DashboardGrainBenchmark
    {
        [Params(10)]
        public int SiloCount { get; set; }

        [Params(50)]
        public int GrainTypeCount { get; set; }

        [Params(10)]
        public int GrainMethodCount { get; set; }

        [Params(100)]
        public int HistorySize { get; set; }

        [ParamsSource(nameof(Histories))]
        public ITraceHistory History { get; set; }

        public IEnumerable<ITraceHistory> Histories
        {
            get
            {
                yield return new TraceHistory(HistorySize);
                yield return new TraceHistoryV2(HistorySize);
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            Setup(History);
        }

        private int time;
        private readonly DateTime startTime = DateTime.UtcNow;
        
        [Benchmark]
        public void Test_Add_TraceHistory()
        {
            var now = startTime.AddSeconds(time++);

            AddTraceData(now, History);
        }
        
        [Benchmark]
        public ICollection Test_QueryAll_TraceHistory()
        {
            return History.QueryAll();
        }

        [Benchmark]
        public ICollection Test_QuerySilo_TraceHistory()
        {
            return History.QuerySilo("SILO_0");
        }

        [Benchmark]
        public ICollection Test_QueryGrain_TraceHistory()
        {
            return History.QueryGrain("GRAIN_0");
        }

        [Benchmark]
        public ICollection Test_GroupByGrainAndSilo_TraceHistory()
        {
            return History.GroupByGrainAndSilo().ToList();
        }
        
        [Benchmark]
        public ICollection Test_AggregateByGrainMethod_TraceHistory()
        {
            return History.AggregateByGrainMethod().ToList();
        }

        private void Setup(ITraceHistory history)
        {
            var start = DateTime.Now.AddSeconds(-HistorySize);

            for (var timeIndex = 0; timeIndex < HistorySize; timeIndex++)
            {
                var time = start.AddSeconds(timeIndex);

                AddTraceData(time, history);
            }
        }

        private void AddTraceData(DateTime time, ITraceHistory history)
        {
            for (var siloIndex = 0; siloIndex < SiloCount; siloIndex++)
            {
                var trace = new List<SiloGrainTraceEntry>();

                for (var grainIndex = 0; grainIndex < GrainTypeCount; grainIndex++)
                {
                    for (var grainMethodIndex = 0; grainMethodIndex < GrainMethodCount; grainMethodIndex++)
                    {
                        trace.Add(new SiloGrainTraceEntry{
                            ElapsedTime = 10,
                            Count = 100,
                            Method = $"METHOD_{grainMethodIndex}",
                            Grain = $"GRAIN_{grainIndex}",
                            ExceptionCount = 0
                        });
                    }
                }

                history.Add(time, $"SILO_{siloIndex}", trace.ToArray());
            }
        }     
    }
}