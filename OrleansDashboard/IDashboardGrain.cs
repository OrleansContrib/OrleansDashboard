using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public interface IDashboardGrain : IGrain, IGrainWithIntegerKey
    {
        Task Init();

        Task<DashboardCounters> GetCounters();

        Task SubmitTracing(string siloIdentity, IDictionary<string, GrainTraceEntry>[] grainCallTime);
    }
}
