using OrleansDashboard.BasicAuthentication;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class BasicAuthenticationAppBuilderExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, BasicAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            return app.UseMiddleware<BasicAuthenticationMiddleware>(options);
        }
    }
}
