using Microsoft.AspNetCore.Http;
using OrleansDashboard.Implementation.Assets;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OrleansDashboard.EmbeddedAssets
{
    public sealed class EmbeddedAssetProvider : IAssetProvider
    {
        private readonly Assembly assembly;

        public EmbeddedAssetProvider()
        {
            assembly = typeof(EmbeddedAssetProvider).Assembly;
        }

        public async Task ServeAssetAsync(string name, HttpContext httpContext)
        {
            var path = $"OrleansDashboard.EmbeddedAssets.Assets.{name.ToLowerInvariant()}";

            var resource = assembly.GetManifestResourceStream(path);

            if (resource != null)
            {
                if (name.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    httpContext.Response.ContentType = "text/css";
                }
                else if (name.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    httpContext.Response.ContentType = "text/javascript";
                }

                await resource.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
            }
            else
            {
                httpContext.Response.StatusCode = 404;
            }
        }
    }
}
