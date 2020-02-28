using System.Net;
using Microsoft.AspNetCore.Http;

namespace OrleansDashboard
{
    internal static class Extensions
    {
        internal static string ToValue(this PathString path)
        {
            return WebUtility.UrlDecode(path.ToString().Substring(1));
        }
    }
}
