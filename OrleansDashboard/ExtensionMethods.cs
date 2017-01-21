using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Orleans.Runtime.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans.Providers;
using Microsoft.AspNetCore.Http;

namespace OrleansDashboard
{
    public static class ExtensionMethods
    {
        public static async Task ReturnFileAsync(this HttpResponse response, string name, string contentType)
        {
            response.ContentType = contentType;
            var assembly = typeof(ExtensionMethods).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream($"OrleansDashboard.{name}"))
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                await response.WriteAsync(content);
            }
        }

        public static Task ReturnJson(this HttpResponse response, object value)
        {
            response.ContentType = "application/json";
            return response.WriteAsync(
                JsonConvert.SerializeObject(value,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }

        public static Task ReturnUnauthorised(this HttpResponse response)
        {
            response.StatusCode = 401;
            response.Headers.Add("WWW-Authenticate", new string[] { "Basic realm=\"OrleansDashboard\"" });
            return Task.FromResult(0);
        }

        public static double SumZero<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (!source.Any()) return 0;
            return source.Sum(selector);
        }

        public static long SumZero<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            if (!source.Any()) return 0;
            return source.Sum(selector);
        }


        public static double AverageZero<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (!source.Any()) return 0;
            return source.Average(selector);
        }


        public static string ToSiloAddress(this string value)
        {
            var parts = value.Split(':');
            return $"{parts[0].Substring(1)}:{parts[1]}@{parts[2]}";
        }

        public static void RegisterDashboard(this GlobalConfiguration config, int port = 8080, string username = null, string password = null)
        {
            var settings = new Dictionary<string, string>();
            settings.Add("Port", port.ToString());
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                settings.Add("Username", username);
                settings.Add("Password", password);
            }
            config.RegisterBootstrapProvider<Dashboard>("Dashboard", settings);
        }


        public static string ToSiloAddress(this IProviderRuntime providerRuntime)
        {
            var parts = providerRuntime.SiloIdentity.Substring(1).Split(':');
            return string.Format("{0}:{1}@{2}", parts[0], parts[1], parts[2]);
        }
    }
}
