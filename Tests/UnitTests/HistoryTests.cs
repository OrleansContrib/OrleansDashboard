using OrleansDashboard.Metrics.History;

namespace UnitTests
{
    public class HistoryTests : TraceHistoryTestBase
    {
        protected override ITraceHistory CreateHistory()
        {
            return new TraceHistory(100);
        }
    }
}