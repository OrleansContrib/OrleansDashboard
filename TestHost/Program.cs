using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.CodeGeneration;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;
using OrleansDashboard;
using TestGrains;

namespace TestHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting silos");
            Console.WriteLine("Dashboard will listen on http://localhost:8080/");

            // Deploy 3 silos
            var options = new TestClusterOptions(3);
            options.ClusterConfiguration.UseStartupType<Startup>();
            options.ClusterConfiguration.Globals.RegisterDashboard();
            var cluster = new TestCluster(options);
            cluster.Deploy();

            // generate some calls to a test grain
            Console.WriteLine("All silos are up and running");

            var tokenSource = new CancellationTokenSource();
            var t = new Thread(() => CallGenerator(cluster.Client, tokenSource).Wait());
            t.Start();

            Console.ReadLine();
            tokenSource.Cancel();
            try
            {
                t.Join(TimeSpan.FromSeconds(3));
            }
            catch
            { }
            cluster.StopAllSilos();
        }

        class Startup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<Func<MethodInfo, InvokeMethodRequest, IGrain, string>>(Format);
                return services.BuildServiceProvider();
            }

            public string Format(MethodInfo targetMethod, InvokeMethodRequest request, IGrain grain)
            {
                if (targetMethod == null)
                    return "Unknown";

                if (grain is TestMessageBasedGrain)
                {
                    var arg = request.Arguments[0];
                    return arg?.GetType().Name ?? $"{targetMethod.Name}(NULL)";
                }

                return targetMethod.Name;
            }
        }

        private static async Task CallGenerator(IClusterClient client, CancellationTokenSource tokenSource)
        {
            var a = client.GetGrain<ITestMessageBasedGrain>(42);
            a.Receive("string").Wait();
            a.ReceiveVoid(DateTime.UtcNow).Wait();
            a.Notify(null).Wait();

            var x = client.GetGrain<ITestGenericGrain<string, int>>("test");
            x.TestT("string").Wait();
            x.TestU(1).Wait();
            x.TestTU("string", 1).Wait();

            var rand = new Random();
            while (!tokenSource.IsCancellationRequested)
            {
                var testGrain = client.GetGrain<ITestGrain>(rand.Next(500));
                await testGrain.ExampleMethod1();
                try
                {
                    await testGrain.ExampleMethod2();
                }
                catch
                { }

                // interceptors are currently broken for generic grains
                // https://github.com/dotnet/orleans/issues/2358
                var genericClient = client.GetGrain<IGenericGrain<string>>("foo");
                genericClient.Echo("hello world").Wait();
            }
        }
    }
}