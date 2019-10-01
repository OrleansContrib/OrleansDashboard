using Orleans.Runtime;

namespace OrleansDashboard.Model
{
    public class SiloDetails
    {
        public int FaultZone { get; set; }
        public string HostName { get; set; }
        public string IAmAliveTime { get; set; }
        public int ProxyPort { get; set; }
        public string RoleName { get; set; }
        public string SiloAddress { get; set; }
        public string SiloName { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public int UpdateZone { get; set; }
        public SiloStatus SiloStatus { get; set; }
    }
}