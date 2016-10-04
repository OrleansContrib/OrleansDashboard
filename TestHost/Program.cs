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

            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });
        }

        static void InitSilo(string[] args = null)
        {
            var hostWrapper = new OrleansHostWrapper(args.Length > 0 && args[0] == "primary");

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
            using (new DevSilo())
            using (new DevSilo())
            using (new DevSilo())
            {
                // generate some calls to a test grain
                Orleans.GrainClient.Initialize(ClientConfiguration.LocalhostSilo());
                Console.WriteLine("Calling test grain");
                var rand = new Random();
                while (true)
                {
                    var client = GrainClient.GrainFactory.GetGrain<ITestGrain>(rand.Next(500));
                    client.ExampleMethod1().Wait();
                    client.ExampleMethod2().Wait();
                }
            }
        }




    }
}
