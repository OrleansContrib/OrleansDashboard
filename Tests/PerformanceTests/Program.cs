#if RELEASE
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
#endif

namespace PerformanceTests
{
    internal class Program
    {
        private static void Main()
        {
#if RELEASE
            BenchmarkRunner.Run<DashboardGrainBenchmark>(
                ManualConfig.Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            new ManualTests().Run();
#endif
        }
    }
}