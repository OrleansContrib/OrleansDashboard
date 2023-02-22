using Orleans.Runtime;
using Orleans.Services;

namespace OrleansDashboard;

public interface ISiloGrainClient : IGrainServiceClient<ISiloGrainService>
{
    ISiloGrainService GrainService(SiloAddress destination);
}