using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class TraceWriter : IDisposable
    {
        private readonly DashboardTraceListener traceListener;
        private readonly HttpContext context;

        public TraceWriter(DashboardTraceListener traceListener, HttpContext context)
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
            await context.Response.WriteAsync(message).ConfigureAwait(false);
            context.Response.Body.Flush();
        }

        public void Dispose()
        {
            traceListener.Remove(Write);
        }
    }
}