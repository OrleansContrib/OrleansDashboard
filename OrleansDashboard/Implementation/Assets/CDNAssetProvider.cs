using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansDashboard.Implementation.Assets
{
    class CDNAssetProvider : IAssetProvider
    {
        private static readonly Dictionary<string, string> Paths = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin-lte.css"] = "//cdnjs.cloudflare.com/ajax/libs/admin-lte/2.3.11/css/AdminLTE.min.css",
            ["bootstrap.css"] = "//maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css",
            ["chart.js"] = "//cdnjs.cloudflare.com/ajax/libs/Chart.js/2.0.1/Chart.min.js",
            ["font-awesome.css"] = "//cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.2/css/all.min.css",
            ["html5shiv.js"] = "//oss.maxcdn.com/html5shiv/3.7.3/html5shiv.min.js",
            ["respond.js"] = "//oss.maxcdn.com/respond/1.4.2/respond.min.js",
            ["skin-purple.css"] = "//cdnjs.cloudflare.com/ajax/libs/admin-lte/2.3.11/css/skins/skin-purple.min.css",
        };

        public Task ServeAssetAsync(string name, HttpContext httpContext)
        {
            if (Paths.TryGetValue(name, out var path))
            {
                httpContext.Response.Redirect(path);
            }
            else
            {
                httpContext.Response.StatusCode = 404;
            }

            return Task.CompletedTask;
        }
    }
}
