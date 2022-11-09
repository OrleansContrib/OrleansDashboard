using OrleansDashboard.Metrics.History;

namespace UnitTests
{
    public class HistoryV2Tests : TraceHistoryTestBase
    {
        protected override ITraceHistory CreateHistory()
        {
            return new TraceHistoryV2(100);
        }
    }
}