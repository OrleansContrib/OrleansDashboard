using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using OrleansDashboard.Model;

namespace OrleansDashboard.Implementation.Grains;

[PreferLocalPlacement]
public sealed class SiloGrainProxy : Grain, ISiloGrainProxy
{
    private readonly ISiloGrainService siloGrainService;

    public SiloGrainProxy(ISiloGrainClient siloGrainClient)
    {
        siloGrainService = siloGrainClient.GrainService(
            SiloAddress.FromParsableString(this.GetPrimaryKeyString())
        );
    }

    public Task SetVersion(string orleans, string host)
    {
        return siloGrainService.SetVersion(orleans, host);
    }

    public Task ReportCounters(Immutable<StatCounter[]> stats)
    {
        return siloGrainService.ReportCounters(stats);
    }

    public Task Enable(bool enabled)
    {
        return siloGrainService.Enable(enabled);
    }

    public Task<Immutable<Dictionary<string, string>>> GetExtendedProperties()
    {
        return siloGrainService.GetExtendedProperties();
    }

    public Task<Immutable<SiloRuntimeStatistics[]>> GetRuntimeStatistics()
    {
        return siloGrainService.GetRuntimeStatistics();
    }

    public Task<Immutable<StatCounter[]>> GetCounters()
    {
        return siloGrainService.GetCounters();
    }
}