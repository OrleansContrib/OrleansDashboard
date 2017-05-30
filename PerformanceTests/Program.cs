using BenchmarkDotNet.Running;

namespace PerformanceTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<DashboardGrainBenchmark>();
        }
    }
}