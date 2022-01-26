using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OrleansDashboard.Implementation
{
    public class TraceWriter : IAsyncDisposable
    {
        private readonly DashboardLogger traceListener;
        private readonly HttpContext context;
        private readonly StreamWriter writer;

        public TraceWriter(DashboardLogger traceListener, HttpContext context)
        {
            this.context = context;

            writer = new StreamWriter(context.Response.Body);

            this.traceListener = traceListener;
            this.traceListener.Add(Write);
        }

        private void Write(EventId eventId, LogLevel level, string message)
        {
            var task = WriteAsync(eventId, level, message);

            task.ConfigureAwait(false);
            task.ContinueWith(_ => { /* noop */ });
        }

        public async Task WriteAsync(string message)
        {
            try
            {
                await writer.WriteAsync(message);
                await writer.WriteAsync("\r\n");

                await writer.FlushAsync();

                await context.Response.Body.FlushAsync();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public async Task WriteAsync(EventId eventId, LogLevel level, string message)
        {
            try
            {
                await writer.WriteAsync($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} {GetLogLevelString(level)}: [{eventId.ToString().PadLeft(8)}] {message}\r\n");

                await writer.FlushAsync();

                await context.Response.Body.FlushAsync();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public ValueTask DisposeAsync()
        {
            traceListener.Remove(Write);

            return writer.DisposeAsync();
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
