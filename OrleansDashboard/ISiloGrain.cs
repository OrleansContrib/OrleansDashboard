using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public interface ISiloGrain : IGrainWithStringKey
    {
        Task<SiloRuntimeStatistics[]> GetRuntimeStatistics();
    }
}
