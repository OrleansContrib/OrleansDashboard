using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class WebServer
    {
        static WebServer()
        {
            Router = new Router();
        }

        public static Router Router { get; private set; }

        Task HandleRequest(IOwinContext context, Func<Task> func)
        {
            var result = Router.Match(context.Request.Path.Value);
            if (null != result)
            {
                return result(context);
            }

            context.Response.StatusCode = 404;
            return Task.FromResult(0);
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use(HandleRequest);
        }
    }

}
