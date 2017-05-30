using OrleansDashboard;
using System;
using System.Threading.Tasks;

namespace PerformanceTests
{
    public class TestDashboardGrain : DashboardGrain
    {
        protected override IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return new EmptyDisposable();
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}