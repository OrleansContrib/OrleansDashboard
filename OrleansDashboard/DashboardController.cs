using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans.Providers;

namespace OrleansDashboard
{
    [Route("")]
    public class DashboardController : ControllerBase
    {
        private readonly TaskScheduler taskScheduler;
        private readonly IProviderRuntime providerRuntime;
        private readonly DashboardTraceListener traceListener;

        public DashboardController(TaskScheduler taskScheduler, IProviderRuntime providerRuntime, DashboardTraceListener traceListener)
        {
            this.traceListener = traceListener;
            this.taskScheduler = taskScheduler;
            this.providerRuntime = providerRuntime;
        }

        private static IActionResult CreateFileResult(string name, string contentType)
        {
            var assembly = typeof(DashboardController).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream($"OrleansDashboard.{name}"))
            using (var reader = new BinaryReader(stream))
            {
                var content = reader.ReadBytes((int)stream.Length);
                return new FileContentResult(content, contentType);
            }
        }

        [HttpGet]
        public Task<IActionResult> Index()
        {
            return Task.FromResult(CreateFileResult("Index.html", "text/html"));
        }

        [HttpGet("index.min.js")]
        public Task<IActionResult> IndexJs()
        {
            return Task.FromResult(CreateFileResult("index.min.js", "application/javascript"));
        }

        [HttpGet("favicon.ico")]
        public Task<IActionResult> Favicon()
        {
            return Task.FromResult(CreateFileResult("favicon.ico", "image/x-icon"));
        }

        [HttpGet("DashboardCounters")]
        public async Task<IActionResult> GetDashboardCounters()
        {
            var grain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(grain.GetCounters).ConfigureAwait(false);
            return Ok(result);
        }


        [HttpGet("HistoricalStats/{address}")]
        public async Task<IActionResult> GetHistoricalStats(string address)
        {
            var grain = providerRuntime.GrainFactory.GetGrain<ISiloGrain>(address);

            var result = await Dispatch(grain.GetRuntimeStatistics).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("SiloProperties/{address}")]
        public async Task<IActionResult> GetSiloExtendedProperties(string address)
        {
            var grain = providerRuntime.GrainFactory.GetGrain<ISiloGrain>(address);

            var result = await Dispatch(grain.GetExtendedProperties).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("GrainStats/{grainName}")]
        public async Task<IActionResult> GetGrainStats(string grainName)
        {
            var grain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(() => grain.GetGrainTracing(grainName)).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("ClusterStats")]
        public async Task<IActionResult> GetClusterStats()
        {
            var grain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(grain.GetClusterTracing).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("SiloStats/{address}")]
        public async Task<IActionResult> GetSiloStats(string address)
        {
            var grain = providerRuntime.GrainFactory.GetGrain<IDashboardGrain>(0);

            var result = await Dispatch(() => grain.GetSiloTracing(address)).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("SiloCounters/{address}")]
        public async Task<IActionResult> GetSiloCounters(string address)
        {
            var grain = providerRuntime.GrainFactory.GetGrain<ISiloGrain>(address);

            var result = await Dispatch(grain.GetCounters).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("Reminders")]
        public Task<IActionResult> GetReminders()
        {
            return GetRemindersByPage(1);
        }

        [HttpGet("Reminders/{page:int}")]
        public async Task<IActionResult> GetRemindersByPage(int page)
        {
            const int pageSize = 25;

            var grain = providerRuntime.GrainFactory.GetGrain<IDashboardRemindersGrain>(0);

            var result = await Dispatch(() => grain.GetReminders(page, pageSize)).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("Trace")]
        public async Task<IActionResult> Trace()
        {
            var token = HttpContext.RequestAborted;

            await Dispatch(async () =>
            {
                using (var writer = new TraceWriter(traceListener, HttpContext))
                {
                    await writer.WriteAsync(@"
   ____       _                        _____            _     _                         _
  / __ \     | |                      |  __ \          | |   | |                       | |
 | |  | |_ __| | ___  __ _ _ __  ___  | |  | | __ _ ___| |__ | |__   ___   __ _ _ __ __| |
 | |  | | '__| |/ _ \/ _` | '_ \/ __| | |  | |/ _` / __| '_ \| '_ \ / _ \ / _` | '__/ _` |
 | |__| | |  | |  __/ (_| | | | \__ \ | |__| | (_| \__ \ | | | |_) | (_) | (_| | | | (_| |
  \____/|_|  |_|\___|\__,_|_| |_|___/ |_____/ \__,_|___/_| |_|_.__/ \___/ \__,_|_|  \__,_|

You are connected to the Orleans Dashboard log streaming service
").ConfigureAwait(false);
                    await writer.WriteAsync($"Silo {providerRuntime.ToSiloAddress()}\r\nTime: {DateTime.UtcNow}\r\n\r\n").ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMinutes(60), token).ConfigureAwait(false);
                    await writer.WriteAsync("Disconnecting after 60 minutes\r\n").ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
            return Ok();
        }

        private Task Dispatch(Func<Task> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None,
                taskScheduler).Result;
        }

        private Task<T> Dispatch<T>(Func<Task<T>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, taskScheduler).Result;
        }
    }
}