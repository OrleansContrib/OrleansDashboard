using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Implementation;
using OrleansDashboard.Implementation.Assets;
using OrleansDashboard.Model;

// ReSharper disable ConvertIfStatementToSwitchStatement
namespace OrleansDashboard
{
    public sealed class DashboardMiddleware
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
        };

        static DashboardMiddleware()
        {
            Options.Converters.Add(new TimeSpanConverter());
        }

        private const int REMINDER_PAGE_SIZE = 50;
        private const int UNAVAILABLE_RETRY_DELAY = 1;
        private readonly IOptions<DashboardOptions> options;
        private readonly DashboardLogger logger;
        private readonly RequestDelegate next;
        private readonly IAssetProvider assetProvider;
        private readonly Lazy<IDashboardClient> lazyClient;
        private IDashboardClient Client => lazyClient.Value;

        public DashboardMiddleware(RequestDelegate next,
            IGrainFactory grainFactory,
            IAssetProvider assetProvider,
            IOptions<DashboardOptions> options,
            DashboardLogger logger)
        {
            this.options = options;
            this.logger = logger;
            this.next = next;
            this.assetProvider = assetProvider;
            // ASP.NET Core uses a single instance of a middleware component to process multiple requests,
            this.lazyClient = new Lazy<IDashboardClient>(
                () => new DashboardClient(grainFactory),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            try
            {
                if (request.Path == "/" || string.IsNullOrEmpty(request.Path))
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

                if (request.Path == "/version")
                {
                    await WriteJson(context,
                        new {version = typeof(DashboardMiddleware).Assembly.GetName().Version.ToString()});

                    return;
                }

                if (request.Path.StartsWithSegments("/webfonts", out var name))
                {
                    await assetProvider.ServeAssetAsync(name.Value[1..], context);

                    return;
                }

                if (request.Path.StartsWithSegments("/assets", out var fontName))
                {
                    await assetProvider.ServeAssetAsync(fontName.Value[1..], context);

                    return;
                }

                if (request.Path == "/DashboardCounters")
                {
                    var result = await Client.DashboardCounters();

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path == "/ClusterStats")
                {
                    var result = await Client.ClusterStats();

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path == "/Reminders")
                {
                    try
                    {
                        var result = await Client.GetReminders(1, REMINDER_PAGE_SIZE);

                        await WriteJson(context, result.Value);
                    }
                    catch
                    {
                        // if reminders are not configured, the call to the grain will fail
                        await WriteJson(context,
                            new ReminderResponse {Reminders = Array.Empty<ReminderInfo>(), Count = 0});
                    }

                    return;
                }

                if (request.Path.StartsWithSegments("/Reminders", out var pageString1) &&
                    int.TryParse(pageString1.ToValue(), out var page))
                {
                    try
                    {
                        var result = await Client.GetReminders(page, REMINDER_PAGE_SIZE);

                        await WriteJson(context, result.Value);
                    }
                    catch
                    {
                        // if reminders are not configured, the call to the grain will fail
                        await WriteJson(context,
                            new ReminderResponse {Reminders = Array.Empty<ReminderInfo>(), Count = 0});
                    }

                    return;
                }

                if (request.Path.StartsWithSegments("/HistoricalStats", out var remaining))
                {
                    var result = await Client.HistoricalStats(remaining.ToValue());

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path.StartsWithSegments("/SiloProperties", out var address1))
                {
                    var result = await Client.SiloProperties(address1.ToValue());

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path.StartsWithSegments("/SiloStats", out var address2))
                {
                    var result = await Client.SiloStats(address2.ToValue());

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path.StartsWithSegments("/SiloCounters", out var address3))
                {
                    var result = await Client.GetCounters(address3.ToValue());

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path.StartsWithSegments("/GrainStats", out var grainName1))
                {
                    var result = await Client.GrainStats(grainName1.ToValue());

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path == "/TopGrainMethods")
                {
                    var result = await Client.TopGrainMethods(take: 5);

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path == "/GrainState")
                {
                    request.Query.TryGetValue("grainId", out var grainId);

                    request.Query.TryGetValue("grainType", out var grainType);

                    var result = await Client.GetGrainState(grainId, grainType);

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path == "/GrainTypes")
                {
                    var result = await Client.GetGrainTypes();

                    await WriteJson(context, result.Value);

                    return;
                }

                if (request.Path == "/GrainState")
                {
                    request.Query.TryGetValue("grainId", out var grainId);

                    request.Query.TryGetValue("grainType", out var grainType);

                    var result = await Client.GetGrainState(grainId, grainType);

                    await WriteJson(context, result);

                    return;
                }

                if (request.Path == "/Trace")
                {
                    await TraceAsync(context);

                    return;
                }
            }
            catch (SiloUnavailableException)
            {
                await WriteUnavailable(context, true);

                return;
            }

            await next(context);
        }

        private static async Task WriteUnavailable(HttpContext context, bool lostConnectivity)
        {
            context.Response.StatusCode = 503; // Service Unavailable
            context.Response.ContentType = "text/plain";
            context.Response.Headers["Retry-After"] = UNAVAILABLE_RETRY_DELAY.ToString();

            if (lostConnectivity)
            {
                await context.Response.WriteAsync("The dashboard has lost connectivity with the Orleans cluster");
            }
            else
            {
                await context.Response.WriteAsync("The dashboard is still trying to connect to the Orleans cluster");
            }
        }

        private static async Task WriteJson<T>(HttpContext context, T content)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/json";

            await using (var writer = new Utf8JsonWriter(context.Response.BodyWriter))
            {
                JsonSerializer.Serialize(writer, content, Options);
            }
        }

        private static async Task WriteFileAsync(HttpContext context, string name, string contentType)
        {
            var assembly = typeof(DashboardMiddleware).GetTypeInfo().Assembly;

            context.Response.StatusCode = 200;
            context.Response.ContentType = contentType;

            var stream = OpenFile(name, assembly);

            await using (stream)
            {
                await stream.CopyToAsync(context.Response.Body);
            }
        }

        private async Task WriteIndexFile(HttpContext context)
        {
            var assembly = typeof(DashboardMiddleware).GetTypeInfo().Assembly;

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";

            var stream = OpenFile("Index.html", assembly);

            await using (stream)
            {
                var content = await new StreamReader(stream).ReadToEndAsync();

                var basePath = string.IsNullOrWhiteSpace(options.Value.ScriptPath)
                    ? context.Request.PathBase.ToString()
                    : options.Value.ScriptPath;

                if (basePath != "/")
                {
                    basePath += "/";
                }

                content = content.Replace("{{BASE}}", basePath);
                content = content.Replace("{{HIDE_TRACE}}", options.Value.HideTrace.ToString().ToLowerInvariant());
                content = content.Replace("{{CUSTOM_CSS}}", options.Value.CustomCssPath switch
                {
                    // We're deliberately not escaping path as we're keep things lightweight and we don't want to bring in other dependencies
                    { } path => $@"<link rel=""stylesheet"" type=""text/css"" href=""{path}"" />",
                    _ => string.Empty
                });

                await context.Response.WriteAsync(content);
            }
        }

        private async Task TraceAsync(HttpContext context)
        {
            if (options.Value.HideTrace)
            {
                context.Response.StatusCode = 403;
                return;
            }

            var token = context.RequestAborted;

            try
            {
                await using var writer = new TraceWriter(logger, context);
                await writer.WriteAsync("""
                           ____       _                        _____            _     _                         _
                          / __ \     | |                      |  __ \          | |   | |                       | |
                         | |  | |_ __| | ___  __ _ _ __  ___  | |  | | __ _ ___| |__ | |__   ___   __ _ _ __ __| |
                         | |  | | '__| |/ _ \/ _` | '_ \/ __| | |  | |/ _` / __| '_ \| '_ \ / _ \ / _` | '__/ _` |
                         | |__| | |  | |  __/ (_| | | | \__ \ | |__| | (_| \__ \ | | | |_) | (_) | (_| | | | (_| |
                          \____/|_|  |_|\___|\__,_|_| |_|___/ |_____/ \__,_|___/_| |_|_.__/ \___/ \__,_|_|  \__,_|
                        
                        You are connected to the Orleans Dashboard log streaming service
                        """)
                    .ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromMinutes(60), token).ConfigureAwait(false);

                await writer.WriteAsync("Disconnecting after 60 minutes\r\n").ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static Stream OpenFile(string name, Assembly assembly)
        {
            var file = new FileInfo(name);

            return file.Exists
                ? file.OpenRead()
                : assembly.GetManifestResourceStream($"OrleansDashboard.{name}");
        }
    }
}