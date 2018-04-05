using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrleansDashboard
{
    public static class ExtensionMethods
    {
        internal static string ToValue(this PathString path)
        {
            return path.ToString().Substring(1);
        }

        internal static string ToPeriodString(this DateTime value)
        {
            return value.ToString("o").Split('.').First();
        }

        internal static string ToISOString(this DateTime value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }
    }
}