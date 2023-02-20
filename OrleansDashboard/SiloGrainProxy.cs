using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using OrleansDashboard.Model;

namespace OrleansDashboard;

[PreferLocalPlacement]
public sealed class SiloGrainProxy : Grain, ISiloGrainProxy
{
    private readonly ISiloGrainClient siloGrainClient;
    private ISiloGrainService siloGrainService;

    public SiloGrainProxy(ISiloGrainClient siloGrainClient)
    {
        this.siloGrainClient = siloGrainClient;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        siloGrainService = siloGrainClient.GrainService(
            SiloAddress.FromParsableString(this.GetPrimaryKeyString())
        );
        return base.OnActivateAsync(cancellationToken);
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

    public Task<Immutable<List<SiloRuntimeStatistics>>> GetRuntimeStatistics()
    {
        return siloGrainService.GetRuntimeStatistics();
    }

    public Task<Immutable<StatCounter[]>> GetCounters()
    {
        return siloGrainService.GetCounters();
    }
}