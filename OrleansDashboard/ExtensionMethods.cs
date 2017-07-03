using Orleans.Providers;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
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

        public static void RegisterDashboard(this GlobalConfiguration config, int port = 8080, string username = null, string password = null)
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
    }
}