using Orleans;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGrains;

namespace TestHost
{
    class DevSilo : IDisposable
    {
        private static List<OrleansHostWrapper> hostWrappers = new List<OrleansHostWrapper>();
        static bool isPrimary = true;
        static int portAllocation = 50000;

        public DevSilo()
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            var args = new string[] {  };
            if (isPrimary)
            {
                isPrimary = false;
                args = new string[] { "primary" };
            }
            else
            {
                args = new string[] { "secondary", (portAllocation++).ToString(), (portAllocation++).ToString() };
            }

            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });
        }

        static void InitSilo(string[] args = null)
        {
            OrleansHostWrapper hostWrapper = null;
            if (args[0] == "primary")
            {
                hostWrapper = new OrleansHostWrapper();
            }
            else
            {
                hostWrapper = new OrleansHostWrapper(int.Parse(args[1]), int.Parse(args[2]));
            }

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
            hostWrappers.Add(hostWrapper);
        }

        public void Dispose()
        {
            foreach (var hostWrapper in hostWrappers)
            {
                if (hostWrapper == null) return;
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting silos");
            Console.WriteLine("Dashboard will listen on http://localhost:8080/");

            using (new DevSilo())
            using (new DevSilo())
            using (new DevSilo())
            {
                // generate some calls to a test grain
                GrainClient.Initialize(ClientConfiguration.LocalhostSilo());
                Console.WriteLine("All silos are up and running");

                var x = GrainClient.GrainFactory.GetGrain<ITestGenericGrain<string, int>>("test");
                x.TestT("string").Wait();
                x.TestU(1).Wait();
                x.TestTU("string", 1).Wait();

                var rand = new Random();
                while (true)
                {
                    var client = GrainClient.GrainFactory.GetGrain<ITestGrain>(rand.Next(500));
                    client.ExampleMethod1().Wait();
                    try
                    {
                        client.ExampleMethod2().Wait();
                    }
                    catch
                    { }

                    // interceptors are currently broken for generic grains
                    // https://github.com/dotnet/orleans/issues/2358
                    //var genericClient = GrainClient.GrainFactory.GetGrain<IGenericGrain<string>>("foo");
                    //genericClient.Echo("hello world").Wait();
                }
            }
        }




    }
}
