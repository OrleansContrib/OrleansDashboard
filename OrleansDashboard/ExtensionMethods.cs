using Orleans.Providers;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrleansDashboard
{
    public static class ExtensionMethods
    {
        internal static string ToSiloAddress(this string value)
        {
            var parts = value.Split(':');
            return $"{parts[0].Substring(1)}:{parts[1]}@{parts[2]}";
        }

        public static void RegisterDashboard(this GlobalConfiguration config, 
            int port = 8080, 
            string username = null, 
            string password = null, 
            string hostName = null,
            int siloSampleFrequency = 0,
            int grainSampleFrequency = 0,
            bool disableTrace = false,
            bool disableProfiling = false)
        {
            var settings = new Dictionary<string, string>
            {
                { "Port", port.ToString() }
            };
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                settings.Add("Username", username);
                settings.Add("Password", password);
            }

            if (!string.IsNullOrWhiteSpace(hostName))
            {
                settings.Add("Host", hostName);
            }

            if (siloSampleFrequency > 0)
            {
                settings.Add("SiloSampleFrequency", siloSampleFrequency.ToString());
            }

            if (grainSampleFrequency > 0)
            {
                settings.Add("GrainSampleFrequency", grainSampleFrequency.ToString());
            }

            if (disableProfiling)
            {
                settings.Add("DisableProfiling", "true");
            }

            if (disableTrace)
            {
                settings.Add("DisableTrace", "true");
            }

            config.RegisterBootstrapProvider<Dashboard>("Dashboard", settings);

            config.RegisterStatisticsProvider<StatsPublisher>("DashboardStats");
        }

        internal static string ToSiloAddress(this IProviderRuntime providerRuntime)
        {
            var parts = providerRuntime.SiloIdentity.Substring(1).Split(':');
            return $"{parts[0]}:{parts[1]}@{parts[2]}";
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