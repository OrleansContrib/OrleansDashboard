using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Text;

namespace OrleansDashboard.BasicAuthentication
{
    public class BasicAuthenticationMiddleware
    {
        private ILogger<BasicAuthenticationMiddleware> _logger;
        private RequestDelegate _next;
        private BasicAuthenticationOptions _options;

        public BasicAuthenticationMiddleware(RequestDelegate next, IOptions<BasicAuthenticationOptions> options, ILoggerFactory loggerFactory)
        {
            _next = next;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<BasicAuthenticationMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            string authorizationHeader = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Replace("Basic", "").Trim()));
                var parts = decodedString.Split(':');
                if (parts.Length == 2)
                {
                    if (parts[0] == _options.Username && parts[1] == _options.Password)
                    {
                        await _next.Invoke(context);
                        return;
                    }
                }
            }

            await context.Response.ReturnUnauthorised();
        }
    }
}
