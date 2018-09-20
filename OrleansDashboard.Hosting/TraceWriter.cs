using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrleansDashboard.Hosting
{
    public class TraceWriter : IDisposable
    {
        private readonly HttpContext _context;
        private readonly DashboardLogger _traceListener;

        public TraceWriter(DashboardLogger traceListener, HttpContext context)
        {
            _traceListener = traceListener;
            _traceListener.Add(Write);
            _context = context;
        }

        public void Dispose()
        {
            _traceListener.Remove(Write);
        }

        private void Write(string message)
        {
            var task = WriteAsync(message);
            task.ConfigureAwait(false);
            task.ContinueWith(_ =>
            {
                /* noop */
            });
        }

        public async Task WriteAsync(string message)
        {
            await _context.Response.WriteAsync(message + "\r\n").ConfigureAwait(false);
            await _context.Response.Body.FlushAsync().ConfigureAwait(false);
        }
    }
}