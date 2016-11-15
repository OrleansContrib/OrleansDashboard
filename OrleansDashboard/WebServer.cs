using Microsoft.Owin;
using Owin;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class WebServer
    {
        public WebServer(Router router, string username, string password)
        {
            this.Router = router;
            this.Username = username;
            this.Password = password;
        }

        public Router Router { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        async Task HandleRequest(IOwinContext context, Func<Task> func)
        {
            var result = this.Router.Match(context.Request.Path.Value);
            if (null == result)
            {
                context.Response.StatusCode = 404;
                return;
            }
            try
            {
                await result(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.ReturnJson(ex);
            }
        }

        Task BasicAuth(IOwinContext context, Func<Task> func)
        {
            if (!context.Request.Headers.ContainsKey("Authorization")) return context.ReturnUnauthorised();

            //Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            var value = context.Request.Headers["Authorization"];

            var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(value.Replace("Basic", "").Trim()));

            var parts = decodedString.Split(':');

            if (parts.Length != 2) return context.ReturnUnauthorised();

            if (parts[0] != this.Username && parts[1] != this.Password) return context.ReturnUnauthorised();

            return func();
        }

        public void Configuration(IAppBuilder app)
        {
            if (!string.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password))
            {
                // if a username and password are supplied, enable basic auth
                app.Use(BasicAuth);
            }
            app.Use(HandleRequest);
        }
    }

}
