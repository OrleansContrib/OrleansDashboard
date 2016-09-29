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

        Task SubmitTracing(GrainTraceEntry[] grainCallTime);

        Task<Dictionary<string, Dictionary<string, GrainTraceEntry>>> GetGrainTracing(string grain);
    }
}
