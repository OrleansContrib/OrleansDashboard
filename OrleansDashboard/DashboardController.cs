using Microsoft.Owin;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{

    public class DashboardController 
    {
        public TaskScheduler TaskScheduler { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }


        public DashboardController(Router router, TaskScheduler taskScheduler, IProviderRuntime providerRuntime)
        {

            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;

            Action<string, Func<IOwinContext, IDictionary<string, string>, Task>> add = router.Add;

            add("/", Index);
            add("/index.min.js", IndexJs);
            add("/DashboardCounters", GetDashboardCounters);
            add("/RuntimeStats/:address", GetRuntimeStats);
            add("/HistoricalStats/:address", GetHistoricalStats);


            //this.Get["/SiloPerformanceMetrics"] = GetSiloPerformanceMetrics;
            //this.Get["/ClientPerformanceMetrics"] = GetClientPerformanceMetrics;
            //this.Get["/Counters"] = GetCounters;
            //add("/ForceActivationCollection/{timespan:int}/{address?}", PostForceActivationCollection);
        }



        Task Index(IOwinContext context, IDictionary<string,string> parameters)
        {
            return context.ReturnFile("Index.html", "text/html");
        }

        Task IndexJs(IOwinContext context, IDictionary<string, string> parameters)
        {
            return context.ReturnFile("index.min.js", "application/javascript");
        }


        /*
        object PostForceActivationCollection(dynamic parameters)
        {
            var grain = Dashboard.ProviderRuntime.GrainFactory.GetGrain<IManagementGrain>(0);
            var timespan = TimeSpan.FromSeconds(parameters.timespan);

            if (parameters.address.HasValue)
            {
                var address = SiloAddress.FromParsableString((string)parameters.address);
                Dispatch(async () =>
                {
                    await grain.ForceActivationCollection(new SiloAddress[] { address }, timespan);
                    return "";
                });
            }
            else
            {
                Dispatch(async () =>
                {
                    await grain.ForceActivationCollection(timespan);
                    return "";
                });
            }

            return this.Response.AsJson(new { });
        }
        */

        async Task GetDashboardCounters(IOwinContext context, IDictionary<string, string> parameters)
        {
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () => {
                return await grain.GetCounters();
            });

            await context.ReturnJson(result);
        }


        async Task GetRuntimeStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var address = SiloAddress.FromParsableString(parameters["address"]);
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IManagementGrain>(0);
            
            var result = await Dispatch(async () =>
            {
                Dictionary<SiloAddress, SiloStatus> silos = await grain.GetHosts(true);
                
                SiloStatus siloStatus;
                if (silos.TryGetValue(address, out siloStatus))
                {
                    return (await grain.GetRuntimeStatistics(new SiloAddress[] { address })).FirstOrDefault();
                }
                return null;
            });


            await context.ReturnJson(result);
        }

        async Task GetHistoricalStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<ISiloGrain>(parameters["address"]);

            var result = await Dispatch(async () =>
            {
                return await grain.GetRuntimeStatistics();
            });

            await context.ReturnJson(result);
        }


        Task<object> Dispatch(Func<Task<object>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.TaskScheduler).Result;
        }

    }
}
