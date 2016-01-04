using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public interface IDashboardGrain : IGrainWithIntegerKey
    {
        Task Init();

        Task<DashboardCounters> GetCounters();
    }
}
