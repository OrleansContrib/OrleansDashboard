using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    internal class BasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly UserCredentials userCredentials;

        public BasicAuthMiddleware(RequestDelegate next, UserCredentials userCredentials)
        {
            this.next = next;
            this.userCredentials = userCredentials;
        }

        public Task Invoke(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(userCredentials.Username) ||
                string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                return next(context);
            }

            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var value = context.Request.Headers["Authorization"].ToString();

                var decodedString =
                    Encoding.UTF8.GetString(Convert.FromBase64String(value.Replace("Basic", "").Trim()));

                var parts = decodedString.Split(':');

                if (parts.Length == 2 && parts[0] == userCredentials.Username && parts[1] == userCredentials.Password)
                {
                    return next(context);
                }
            }
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Unauthorized";
            context.Response.Headers.Add("WWW-Authenticate", new[] { "Basic realm=\"OrleansDashboard\"" });

            return Task.CompletedTask;
        }
    }
}