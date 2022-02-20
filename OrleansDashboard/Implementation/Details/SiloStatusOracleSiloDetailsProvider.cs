using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using OrleansDashboard.Metrics.Details;
using OrleansDashboard.Model;

namespace OrleansDashboard.Implementation.Details
{
    public sealed class SiloStatusOracleSiloDetailsProvider : ISiloDetailsProvider
    {
        private readonly ISiloStatusOracle siloStatusOracle;

        public SiloStatusOracleSiloDetailsProvider(ISiloStatusOracle siloStatusOracle)
        {
            this.siloStatusOracle = siloStatusOracle;
        }

        public Task<SiloDetails[]> GetSiloDetails()
        {
            return Task.FromResult(siloStatusOracle.GetApproximateSiloStatuses(true)
                .Select(x => new SiloDetails
                {
                    Status = x.Value.ToString(),
                    SiloStatus = x.Value,
                    SiloAddress = x.Key.ToParsableString(),
                    SiloName = x.Key.ToParsableString() // Use the address for naming
                })
                .ToArray());
        }
    }
}