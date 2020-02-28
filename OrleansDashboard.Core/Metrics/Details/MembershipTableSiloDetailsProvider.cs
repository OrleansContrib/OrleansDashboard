using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Model;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansDashboard.Metrics.Details
{
    /// <summary>
    /// Default silo details provider
    /// Uses IManagementGrain internally.
    /// <remarks>Do not use if there is no membershiptable. use <see cref="SiloStatusOracleSiloDetailsProvider"/> 
    /// instead.</remarks>
    /// </summary>
    public sealed class MembershipTableSiloDetailsProvider : ISiloDetailsProvider
    {
        private readonly IGrainFactory grainFactory;

        public MembershipTableSiloDetailsProvider(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<SiloDetails[]> GetSiloDetails()
        {
            //default implementation uses managementgrain details
            var grain = grainFactory.GetGrain<IManagementGrain>(0);

            var hosts = await grain.GetDetailedHosts(true);

            return hosts.Select(x => new SiloDetails
            {
                FaultZone = x.FaultZone,
                HostName = x.HostName,
                IAmAliveTime = x.IAmAliveTime.ToISOString(),
                ProxyPort = x.ProxyPort,
                RoleName = x.RoleName,
                SiloAddress = x.SiloAddress.ToParsableString(),
                SiloName = x.SiloName,
                StartTime = x.StartTime.ToISOString(),
                Status = x.Status.ToString(),
                SiloStatus = x.Status,
                UpdateZone = x.UpdateZone
            }).ToArray();
        }
    }
}