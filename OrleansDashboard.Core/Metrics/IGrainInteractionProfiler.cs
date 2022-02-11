using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.Model;

namespace OrleansDashboard.Metrics
{
    public interface IInteractionProfiler : IGrainWithIntegerKey
    {
        [OneWay]
        Task Track(GrainInteractionInfoEntry entry);
    }
}
