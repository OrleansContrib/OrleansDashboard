using System.Collections.Immutable;
using System.Linq;

namespace OrleansDashboard.Client.Model
{
    public class DashboardCounters
    {
        public DashboardCounters()
        {
            Hosts = new SiloDetails[0];
            SimpleGrainStats = new SimpleGrainStatisticCounter[0];
            TotalActivationCountHistory = ImmutableQueue<int>.Empty;
            TotalActiveHostCountHistory = ImmutableQueue<int>.Empty;

            //FIXEME: USE CONST Dashboard.HistoryLength
            foreach (var x in Enumerable.Range(1, 100))
            {
                TotalActivationCountHistory = TotalActivationCountHistory.Enqueue(0);
                TotalActiveHostCountHistory = TotalActiveHostCountHistory.Enqueue(0);
            }
        }

        public int TotalActiveHostCount { get; set; }
        public ImmutableQueue<int> TotalActiveHostCountHistory { get; set; }
        public SiloDetails[] Hosts { get; set; }
        public SimpleGrainStatisticCounter[] SimpleGrainStats { get; set; }
        public int TotalActivationCount { get; set; }
        public ImmutableQueue<int> TotalActivationCountHistory { get; set; }
    }
}