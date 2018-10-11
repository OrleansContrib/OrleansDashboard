﻿using Orleans;
using Orleans.Runtime;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace OrleansDashboard
{
    public static class Extensions
    {
        public static string PrimaryKeyAsString(this GrainReference grainRef)
        {
            if (grainRef.IsPrimaryKeyBasedOnLong()) // Long
            {
                var longKey = grainRef.GetPrimaryKeyLong(out var longExt);

                return longExt != null ? $"{longKey} + {longExt}" : longKey.ToString();
            }

            var stringKey = grainRef.GetPrimaryKeyString();

            if (stringKey == null) // Guid
            {
                var guidKey = grainRef.GetPrimaryKey(out var guidExt).ToString();

                return guidExt != null ? $"{guidKey} + {guidExt}" : guidKey;
            }

            return stringKey;
        }

        internal static string ToValue(this PathString path)
        {
            return WebUtility.UrlDecode(path.ToString().Substring(1));
        }

        internal static string ToPeriodString(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        internal static string ToISOString(this DateTime value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }
    }
}
