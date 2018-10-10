using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard
{
    public interface ISiloDetailsProvider
    {
        Task<SiloDetails[]> GetSiloDetails();
    }

    /// <summary>
    /// Simple silo details provider
    /// Uses ISiloStatusOracle internally
    /// </summary>
    public sealed class SiloStatusOracleSiloDetailsProvider : ISiloDetailsProvider
    {
        private readonly ISiloStatusOracle siloStatusOracle;

        public SiloStatusOracleSiloDetailsProvider(ISiloStatusOracle siloStatusOracle)
        {
            this.siloStatusOracle = siloStatusOracle;
        }

        public Task<SiloDetails[]> GetSiloDetails()
        {
            // todo this could be improved by using a ISiloStatusListener
            // and caching / projecting the changes instead of polling
            // should reduce allocations of array's etc

            return Task.FromResult(siloStatusOracle.GetApproximateSiloStatuses(true)
                .Select(x => new SiloDetails()
                {
                    Status = x.Value.ToString(),
                    SiloStatus = x.Value,
                    SiloAddress = x.Key.ToParsableString(),
                    SiloName = x.Key.ToParsableString() //use the address for naming
                })
                .ToArray());
        }
    }

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