using Orleans.Runtime;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Owin;
using System.Collections.Generic;
using Newtonsoft.Json;
using Orleans.Providers;
using Newtonsoft.Json.Serialization;

namespace OrleansDashboard
{

    public static class ExtensionMethods
    {
        public static async Task ReturnFile(this IOwinContext context, string name, string contentType)
        {
            context.Response.ContentType = contentType;
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($"OrleansDashboard.{name}"))
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                await context.Response.WriteAsync(content);
            }
        }

        public static Task ReturnJson(this IOwinContext context, object value)
        {
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(
                JsonConvert.SerializeObject(value, 
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }

    }

    public class DashboardModule 
    {
        public TaskScheduler TaskScheduler { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }


        public DashboardModule(Router router, TaskScheduler taskScheduler, IProviderRuntime providerRuntime)
        {

            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;

            Action<string, Func<IOwinContext, IDictionary<string, string>, Task>> add = router.Add;

            add("/", Index);
            add("/index.min.js", IndexJs);
            add("/DashboardCounters", GetDashboardCounters);
            add("/RuntimeStats/:address", GetRuntimeStats);

            
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

            var result = await Dispatch(async () => {
                return (await grain.GetRuntimeStatistics(new SiloAddress[] { address })).FirstOrDefault();
            });

            await context.ReturnJson(result);
        }


        Task<object> Dispatch(Func<Task<object>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.TaskScheduler).Result;
        }

    }
}
