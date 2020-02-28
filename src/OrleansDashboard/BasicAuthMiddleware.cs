﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace OrleansDashboard
{
    internal class BasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly DashboardOptions options;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<DashboardOptions> options)
        {
            this.next = next;
            this.options = options.Value;
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var value = context.Request.Headers["Authorization"].ToString();

                var decodedString =
                    Encoding.UTF8.GetString(Convert.FromBase64String(value.Replace("Basic", "").Trim()));

                var parts = decodedString.Split(':');

                if (parts.Length == 2 && parts[0] == options.Username && parts[1] == options.Password)
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