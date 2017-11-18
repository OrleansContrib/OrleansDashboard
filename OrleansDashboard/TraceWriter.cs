using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class TraceWriter : IDisposable
    {
        private readonly DashboardLogger traceListener;
        private readonly HttpContext context;

        public TraceWriter(DashboardLogger traceListener, HttpContext context)
        {
            this.traceListener = traceListener;
            this.traceListener.Add(Write);
            this.context = context;
        }

        private void Write(string message)
        {
            WriteAsync(message).Wait();
        }

        public async Task WriteAsync(string message)
        {
            await context.Response.WriteAsync(message + "\r\n").ConfigureAwait(false);
            await context.Response.Body.FlushAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            traceListener.Remove(Write);
        }
    }
}