using Orleans.Runtime;
using Orleans.Services;
using OrleansDashboard;

public interface ISiloGrainClient : IGrainServiceClient<ISiloGrainService>
{
    ISiloGrainService GrainService(SiloAddress destination);
}