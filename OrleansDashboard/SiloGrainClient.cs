using System;
using Orleans.Runtime;
using Orleans.Runtime.Services;

namespace OrleansDashboard;

public sealed class SiloGrainClient : GrainServiceClient<ISiloGrainService>, ISiloGrainClient
{
    public SiloGrainClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public ISiloGrainService GrainService(SiloAddress destination)
    {
        return GetGrainService(destination);
    }
}