using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using OrleansDashboard.Model;
using OrleansDashboard.Metrics.History;

namespace PerformanceTests
{
    public class DashboardGrainBenchmark
    {

        [Params(10)]
        public int SiloCount { get; set; }

        [Params(50)]
        public int GrainTypeCount { get; set; }

        [Params(10)]
        public int GrainMethodCount { get; set; }

        [Params(100)]
        public int HistorySize {get;set;}

        [GlobalSetup]
        public void Setup()
        {
            Setup(traceHistory);
        }

        // multiple implementations of trace history could be tested here
        readonly ITraceHistory traceHistory = new TraceHistory();
        private int time;
        private DateTime startTime = DateTime.UtcNow;
        

        [Benchmark]
        public void Test_Add_TraceHistory()
        {
            var now = startTime.AddSeconds(time++);
            AddTraceData(now, traceHistory);
        }
        
        [Benchmark]
        public void Test_QueryAll_TraceHistory()
        {
            traceHistory.QueryAll();
        }

        [Benchmark]
        public void Test_QuerySilo_TraceHistory()
        {
            traceHistory.QuerySilo("SILO_0");
        }

        [Benchmark]
        public void Test_QueryGrain_TraceHistory()
        {
            traceHistory.QueryGrain("GRAIN_0");
        }

        [Benchmark]
        public void Test_GroupByGrainAndSilo_TraceHistory()
        {
            traceHistory.GroupByGrainAndSilo().ToLookup(x => (x.Grain, x.SiloAddress));
        }

        
        [Benchmark]
        public void Test_AggregateByGrainMethod_TraceHistory()
        {
            traceHistory.AggregateByGrainMethod().ToList();
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