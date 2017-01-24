using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Orleans;
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
        private readonly TaskScheduler _taskScheduler;
        private readonly IProviderRuntime _providerRuntime;

        public DashboardController(IProviderRuntime providerRuntime, TaskScheduler taskScheduler)
        {
            _providerRuntime = providerRuntime;
            _taskScheduler = taskScheduler;
        }

        public Task Index(HttpContext context, RouteValueDictionary routeValues)
        {
            return context.Response.ReturnFileAsync("Index.html", "text/html");
        }

        public Task IndexJs(HttpContext context, RouteValueDictionary routeValues)
        {
            return context.Response.ReturnFileAsync("index.min.js", "application/javascript");
        }

        public async Task GetDashboardCounters(HttpContext context, RouteValueDictionary routeValues)
        {
            var grain = _providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () => {
                return await grain.GetCounters();
            });
            await context.Response.ReturnJson(result);
        }

        public async Task GetRuntimeStats(HttpContext context, RouteValueDictionary routeValues)
        {
            var address = SiloAddress.FromParsableString(routeValues["id"].ToString());
            var grain = this._providerRuntime.GrainFactory.GetGrain<IManagementGrain>(0);
            
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


            await context.Response.ReturnJson(result);
        }

        public async Task GetHistoricalStats(HttpContext context, RouteValueDictionary routeValues)
        {
            var grain = this._providerRuntime.GrainFactory.GetGrain<ISiloGrain>(routeValues["id"].ToString());

            var result = await Dispatch(async () =>
            {
                return await grain.GetRuntimeStatistics();
            });

            await context.Response.ReturnJson(result);
        }

        public async Task GetSiloExtendedProperties(HttpContext context, RouteValueDictionary routeValues)
        {
            var grain = _providerRuntime.GrainFactory.GetGrain<ISiloGrain>(routeValues["id"].ToString());

            var result = await Dispatch(async () =>
            {
                return await grain.GetExtendedProperties();
            });

            await context.Response.ReturnJson(result);
        }

        public async Task GetGrainStats(HttpContext context, RouteValueDictionary routeValues)
        {
            var grainName = routeValues["id"].ToString();
            var grain = this._providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () =>
            {
                return await grain.GetGrainTracing(grainName);
            });

            await context.Response.ReturnJson(result);
        }

        Task<object> Dispatch(Func<Task<object>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: _taskScheduler).Result;
        }

    }
}
