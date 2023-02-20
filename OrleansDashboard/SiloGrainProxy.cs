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
    private ISiloGrainService siloGrainProxyImplementation;

    public SiloGrainProxy(ISiloGrainClient siloGrainClient)
    {
        this.siloGrainClient = siloGrainClient;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        siloGrainProxyImplementation = siloGrainClient.GrainService(
            SiloAddress.FromParsableString(this.GetPrimaryKeyString())
        );
        return base.OnActivateAsync(cancellationToken);
    }

    public Task SetVersion(string orleans, string host)
    {
        return siloGrainProxyImplementation.SetVersion(orleans, host);
    }

    public Task ReportCounters(Immutable<StatCounter[]> stats)
    {
        return siloGrainProxyImplementation.ReportCounters(stats);
    }

    public Task Enable(bool enabled)
    {
        return siloGrainProxyImplementation.Enable(enabled);
    }

    public Task<Immutable<Dictionary<string, string>>> GetExtendedProperties()
    {
        return siloGrainProxyImplementation.GetExtendedProperties();
    }

    public Task<Immutable<List<SiloRuntimeStatistics>>> GetRuntimeStatistics()
    {
        return siloGrainProxyImplementation.GetRuntimeStatistics();
    }

    public Task<Immutable<StatCounter[]>> GetCounters()
    {
        return siloGrainProxyImplementation.GetCounters();
    }
}