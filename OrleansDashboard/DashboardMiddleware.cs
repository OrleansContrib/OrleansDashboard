using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orleans;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace OrleansDashboard
{
    public sealed class DashboardMiddleware
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly IExternalDispatcher dispatcher;
        private readonly IGrainFactory grainFactory;
        private readonly RequestDelegate next;

        public DashboardMiddleware(RequestDelegate next, IGrainFactory grainFactory, IExternalDispatcher dispatcher)
        {
            this.next = next;
            this.grainFactory = grainFactory;
            this.dispatcher = dispatcher;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            if (request.Path == "/")
            {
                await WriteIndexFile(context);

                return;
            }
            if (request.Path == "/favicon.ico")
            {
                await WriteFileAsync(context, "favicon.ico", "image/x-icon");

                return;
            }
            if (request.Path == "/index.min.js")
            {
                await WriteFileAsync(context, "index.min.js", "application/javascript");

                return;
            }

            if (request.Path == "/DashboardCounters")
            {
                var grain = grainFactory.GetGrain<IDashboardGrain>(0);
                var result = await dispatcher.DispatchAsync(grain.GetCounters).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path == "/ClusterStats")
            {
                var grain = grainFactory.GetGrain<IDashboardGrain>(0);
                var result = await dispatcher.DispatchAsync(grain.GetClusterTracing).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path == "/Reminders")
            {
                var grain = grainFactory.GetGrain<IDashboardRemindersGrain>(0);
                var result = await dispatcher.DispatchAsync(() => grain.GetReminders(1, 25)).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path.StartsWithSegments("/Reminders", out var pageString1) && int.TryParse(pageString1.ToValue(), out var page))
            {
                var grain = grainFactory.GetGrain<IDashboardRemindersGrain>(0);
                var result = await dispatcher.DispatchAsync(() => grain.GetReminders(page, 25)).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path.StartsWithSegments("/HistoricalStats", out var remaining))
            {
                var grain = grainFactory.GetGrain<ISiloGrain>(remaining.ToValue());
                var result = await dispatcher.DispatchAsync(grain.GetRuntimeStatistics).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path.StartsWithSegments("/SiloProperties", out var address1))
            {
                var grain = grainFactory.GetGrain<ISiloGrain>(address1.ToValue());
                var result = await dispatcher.DispatchAsync(grain.GetExtendedProperties).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path.StartsWithSegments("/SiloStats", out var address2))
            {
                var grain = grainFactory.GetGrain<IDashboardGrain>(0);
                var result = await dispatcher.DispatchAsync(() => grain.GetGrainTracing(address2.ToValue())).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path.StartsWithSegments("/SiloCounters", out var address3))
            {
                var grain = grainFactory.GetGrain<ISiloGrain>(address3.ToValue());
                var result = await dispatcher.DispatchAsync(grain.GetCounters).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            if (request.Path.StartsWithSegments("/GrainStats", out var grainName1))
            {
                var grain = grainFactory.GetGrain<IDashboardGrain>(0);
                var result = await dispatcher.DispatchAsync(() => grain.GetGrainTracing(grainName1.ToValue())).ConfigureAwait(false);

                await WriteJson(context, result);

                return;
            }

            await next(context);
        }

        private static async Task WriteJson(HttpContext context, object content)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/json";

            var json = JsonConvert.SerializeObject(content, Formatting.Indented, SerializerSettings);

            await context.Response.WriteAsync(json);
        }

        private static async Task WriteFileAsync(HttpContext context, string name, string contentType)
        {
            var assembly = typeof(DashboardController).GetTypeInfo().Assembly;

            context.Response.StatusCode = 200;
            context.Response.ContentType = contentType;

            var stream = OpenFile(name, assembly);

            using (stream)
            {
                await stream.CopyToAsync(context.Response.Body);
            }
        }

        private static async Task WriteIndexFile(HttpContext context)
        {
            var assembly = typeof(DashboardController).GetTypeInfo().Assembly;

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";

            var stream = OpenFile("Index.html", assembly);

            using (stream)
            {
                var content = new StreamReader(stream).ReadToEnd();

                var basePath = context.Request.PathBase;

                if (basePath != "/")
                {
                    basePath = basePath + "/";
                }

                content = content.Replace("{{BASE}}", basePath);

                await context.Response.WriteAsync(content);
            }
        }

        private static Stream OpenFile(string name, Assembly assembly)
        {
            return assembly.GetManifestResourceStream($"OrleansDashboard.{name}");
        }
    }
}