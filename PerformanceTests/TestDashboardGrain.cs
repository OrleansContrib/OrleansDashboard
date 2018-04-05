using OrleansDashboard;
using System;

namespace PerformanceTests
{
    public class TestDashboardGrain : DashboardGrain
    {
        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}