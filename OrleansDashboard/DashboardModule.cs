using Nancy;
using Nancy.Responses;
using Orleans;
using Orleans.Runtime;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace OrleansDashboard
{
    public class DashboardModule : NancyModule
    {
        public DashboardModule()
        {
            this.Get["/"] = Index;
            this.Get["/index.min.js"] = IndexJs;
            //this.Get["/SiloPerformanceMetrics"] = GetSiloPerformanceMetrics;
            //this.Get["/ClientPerformanceMetrics"] = GetClientPerformanceMetrics;
            //this.Get["/Counters"] = GetCounters;
            this.Get["/DashboardCounters"] = GetDashboardCounters;
            this.Get["/RuntimeStats/{address}"] = GetRuntimeStats;
        }

        StreamResponse ReturnFile(string name, string contentType)
        {
            var func = new Func<Stream>(() => 
            {
                var assembly = Assembly.GetExecutingAssembly();
                return assembly.GetManifestResourceStream($"OrleansDashboard.{name}");
            });
            return new StreamResponse(func, contentType);
        }

        StreamResponse Index(dynamic parameters)
        {
            return ReturnFile("Index.html", "text/html");
        }

        StreamResponse IndexJs(dynamic parameters)
        {
            return ReturnFile("index.min.js", "application/javascript");
        }

        /*
        object GetSiloPerformanceMetrics(dynamic parameters)
        {
            return this.Response.AsJson(StatsPublisher.SiloPerformanceMetrics);
        }

        object GetClientPerformanceMetrics(dynamic parameters)
        {
            return this.Response.AsJson(StatsPublisher.ClientPerformanceMetrics);
        }

        object GetCounters(dynamic parameters)
        {
            return this.Response.AsJson(StatsPublisher.Counters);
        }
        */

        object GetDashboardCounters(dynamic parameters)
        {
            var grain = Dashboard.ProviderRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = Dispatch(async () => {
                return await grain.GetCounters();
            });

            return this.Response.AsJson(result);
        }


        object GetRuntimeStats(dynamic parameters)
        {
            var address = SiloAddress.FromParsableString((string) parameters.address);
            var grain = Dashboard.ProviderRuntime.GrainFactory.GetGrain<IManagementGrain>(0);

            var result = Dispatch(async () => {
                return (await grain.GetRuntimeStatistics(new SiloAddress[] { address })).FirstOrDefault();
            });

            return this.Response.AsJson(result);
        }


        object Dispatch(Func<Task<object>> func)
        {
            var result = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: Dashboard.OrleansTS).Result;
            return result.Result;
        }

    }
}
