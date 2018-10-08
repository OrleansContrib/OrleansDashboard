using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard.Client
{
    public interface ISiloGrain : IGrainWithStringKey
    {
        [OneWay]
        Task SetVersion(string orleans, string host);

        [OneWay]
        Task ReportCounters(Immutable<StatCounter[]> stats);

        Task<Immutable<Dictionary<string,string>>> GetExtendedProperties();

        Task<Immutable<SiloRuntimeStatistics[]>> GetRuntimeStatistics();

        Task<Immutable<StatCounter[]>> GetCounters();
    }
}
