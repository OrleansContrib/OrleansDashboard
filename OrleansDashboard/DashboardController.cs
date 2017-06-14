using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Orleans.Providers;
using Orleans.Runtime;

namespace OrleansDashboard
{

    public class DashboardController : IDisposable
    {
        public TaskScheduler TaskScheduler { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        DashboardTraceListener traceListener;

        public DashboardController(Router router, TaskScheduler taskScheduler, IProviderRuntime providerRuntime, DashboardTraceListener traceListener)
        {
            this.traceListener = traceListener;
            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;

            Action<string, Func<IOwinContext, IDictionary<string, string>, Task>> add = router.Add;

            add("/", Index);
            add("/index.min.js", IndexJs);
            add("/favicon.ico", Favicon);
            add("/DashboardCounters", GetDashboardCounters);
            add("/RuntimeStats/:address", GetRuntimeStats);
            add("/HistoricalStats/:address", GetHistoricalStats);
            add("/GrainStats/:grain", GetGrainStats);
            add("/SiloProperties/:address", GetSiloExtendedProperties);
            add("/Trace", Trace);
            add("/ClusterStats", GetClusterStats);
            add("/SiloStats/:address", GetSiloStats);
            add("/SiloCounters/:address", GetSiloCounters);
            add("/Reminders", GetReminders);
        }

        Task Index(IOwinContext context, IDictionary<string,string> parameters)
        {
            return context.ReturnFile("Index.html", "text/html");
        }

        Task IndexJs(IOwinContext context, IDictionary<string, string> parameters)
        {
            return context.ReturnFile("index.min.js", "application/javascript");
        }

        Task Favicon(IOwinContext context, IDictionary<string, string> parameters)
        {
            return context.ReturnBinaryFile("favicon.ico", "image/x-icon");
        }

        async Task GetDashboardCounters(IOwinContext context, IDictionary<string, string> parameters)
        {
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () => {
                return await grain.GetCounters().ConfigureAwait(false);
            }).ConfigureAwait(false);
            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetRuntimeStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var address = SiloAddress.FromParsableString(EscapeString(parameters["address"]));
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IManagementGrain>(0);
            
            var result = await Dispatch(async () =>
            {
                Dictionary<SiloAddress, SiloStatus> silos = await grain.GetHosts(true).ConfigureAwait(false);
                SiloStatus siloStatus;
                if (silos.TryGetValue(address, out siloStatus))
                {
                    return (await grain.GetRuntimeStatistics(new SiloAddress[] { address }).ConfigureAwait(false)).FirstOrDefault();
                }
                return null;
            }).ConfigureAwait(false);


            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetHistoricalStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var address = EscapeString(parameters["address"]);
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<ISiloGrain>(address);

            var result = await Dispatch(async () =>
            {
                return await grain.GetRuntimeStatistics().ConfigureAwait(false);
            }).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetSiloExtendedProperties(IOwinContext context, IDictionary<string, string> parameters)
        {
            var address = EscapeString(parameters["address"]);
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<ISiloGrain>(address);

            var result = await Dispatch(async () =>
            {
                return await grain.GetExtendedProperties().ConfigureAwait(false);
            }).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetGrainStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var grainName = EscapeString(parameters["grain"]);
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () =>
            {
                return await grain.GetGrainTracing(grainName).ConfigureAwait(false);
            }).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetClusterStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () =>
            {
                return await grain.GetClusterTracing().ConfigureAwait(false);
            }).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetSiloStats(IOwinContext context, IDictionary<string, string> parameters)
        {
            var address = EscapeString(parameters["address"]);
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(async () =>
            {
                return await grain.GetSiloTracing(address).ConfigureAwait(false);
            }).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetSiloCounters(IOwinContext context, IDictionary<string, string> parameters)
        {
            var address = EscapeString(parameters["address"]);
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<ISiloGrain>(address);

            var result = await Dispatch(async () =>
            {
                return await grain.GetCounters().ConfigureAwait(false);
            }).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task GetReminders(IOwinContext context, IDictionary<string, string> parameters)
        {            
            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IDashboardRemindersGrain>(0);

            var result = await Dispatch(grain.GetReminders).ConfigureAwait(false);

            await context.ReturnJson(result).ConfigureAwait(false);
        }

        async Task Trace(IOwinContext context, IDictionary<string, string> parameters)
        {
            context.Response.Protocol = "HTTP/1.1";
            await Dispatch(async () => {

                using (var writer = new TraceWriter(this.traceListener, context))
                {
                    writer.Write(@"
   ____       _                        _____            _     _                         _ 
  / __ \     | |                      |  __ \          | |   | |                       | |
 | |  | |_ __| | ___  __ _ _ __  ___  | |  | | __ _ ___| |__ | |__   ___   __ _ _ __ __| |
 | |  | | '__| |/ _ \/ _` | '_ \/ __| | |  | |/ _` / __| '_ \| '_ \ / _ \ / _` | '__/ _` |
 | |__| | |  | |  __/ (_| | | | \__ \ | |__| | (_| \__ \ | | | |_) | (_) | (_| | | | (_| |
  \____/|_|  |_|\___|\__,_|_| |_|___/ |_____/ \__,_|___/_| |_|_.__/ \___/ \__,_|_|  \__,_|
                                                                                          
You are connected to the Orleans Dashboard log streaming service
");
                    writer.Write($"Silo {this.ProviderRuntime.ToSiloAddress()}\r\nTime: {DateTime.UtcNow.ToString()}\r\n\r\n");
                    await Task.Delay(TimeSpan.FromMinutes(60), cancellationTokenSource.Token);
                    writer.Write("Disonnecting after 60 minutes\r\n");
                }

            });
        }

        Task Dispatch(Func<Task> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, 
                TaskScheduler).Result;
        }

        Task<T> Dispatch<T>(Func<Task<T>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, TaskScheduler).Result;
        }

        static string EscapeString(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value
                .Replace("%3C", "<")
                .Replace("%20", " ")
                .Replace("%3E", ">");
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }


}
