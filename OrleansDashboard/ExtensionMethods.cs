using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace OrleansDashboard
{
    public static class ExtensionMethods
    {
        public static void RegisterDashboard(this GlobalConfiguration config, int port = 8080, string username = null, string password = null, string hostName = null)
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

            config.RegisterBootstrapProvider<Dashboard>("Dashboard", settings);
            config.RegisterStatisticsProvider<StatsPublisher>("DashboardStats");
        }

        internal static string ToValue(this PathString path)
        {
            return path.ToString().Substring(1);
        }

        internal static string ToSiloAddress(this string value)
        {
            var parts = value.Split(':');

            return $"{parts[0].Substring(1)}:{parts[1]}@{parts[2]}";
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