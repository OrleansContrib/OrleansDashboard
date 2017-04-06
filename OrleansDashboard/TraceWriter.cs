using Microsoft.Owin;
using System;

namespace OrleansDashboard
{
    public class TraceWriter : IDisposable
    {

        DashboardTraceListener traceListener;
        IOwinContext context;

        public TraceWriter(DashboardTraceListener traceListener, IOwinContext context)
        {
            this.traceListener = traceListener;
            this.traceListener.Add(this.Write);
            this.context = context;

        }

        public void Write(string message)
        {
            this.context.Response.Write(message);
            this.context.Response.Body.Flush();
        }


        public void Dispose()
        {
            this.traceListener.Remove(this.Write);
        }
    }

}
