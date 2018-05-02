using BenchmarkDotNet.Running;
using System;

namespace PerformanceTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<DashboardGrainBenchmark>();

            Console.ReadLine();
        }
    }
}