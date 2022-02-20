using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace PerformanceTests
{
    internal class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<DashboardGrainBenchmark>(
                ManualConfig.Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
        }
    }
}