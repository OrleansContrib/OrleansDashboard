using Microsoft.Extensions.Options;
using OrleansDashboard;
using System;

namespace PerformanceTests
{
    public class TestDashboardGrain : DashboardGrain
    {
        public TestDashboardGrain(IOptions<DashboardOptions> options, ISiloDetailsProvider siloDetailsProvider) 
            : base(options, siloDetailsProvider)
        {
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}