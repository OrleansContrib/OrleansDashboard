using Orleans;

namespace OrleansDashboard;

public interface ISiloGrainProxy : IGrainWithStringKey, ISiloGrainService
{
}