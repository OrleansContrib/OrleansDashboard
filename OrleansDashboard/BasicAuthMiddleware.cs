using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace OrleansDashboard;

internal sealed class BasicAuthMiddleware
{
    private const string BasicAuthorizationPrefix = "Basic ";
    private readonly RequestDelegate next;
    private readonly DashboardOptions options;
    internal static readonly string[] WWWAuthenticateHeaderValue = ["Basic realm=\"OrleansDashboard\""];

    public BasicAuthMiddleware(RequestDelegate next, IOptions<DashboardOptions> options)
    {
        this.next = next;
        this.options = options.Value;
    }

    public Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader) &&
            authorizationHeader[0]!.StartsWith(BasicAuthorizationPrefix, StringComparison.InvariantCulture))
        {
            var authorizationEncoded = authorizationHeader[0][BasicAuthorizationPrefix.Length..].Trim();
            var authorizationBytes = Convert.FromBase64String(authorizationEncoded);

            var decodedSpan = Encoding.UTF8.GetString(authorizationBytes).AsSpan();
            var separatorIndex = decodedSpan.IndexOf(":");

            if (separatorIndex > 0 &&
                decodedSpan[..separatorIndex].SequenceEqual(options.Username) &&
                decodedSpan[(separatorIndex + 1)..].SequenceEqual(options.Password))
            {
                return next(context);
            }
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = ReasonPhrases.GetReasonPhrase(context.Response.StatusCode);
        context.Response.Headers[HeaderNames.WWWAuthenticate] = WWWAuthenticateHeaderValue;

        return Task.CompletedTask;
    }
}
