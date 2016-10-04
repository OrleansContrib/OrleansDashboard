using System;
using System.Net;
using System.Threading.Tasks;

using Orleans.Runtime.Host;
using System.Reflection;
using System.IO;
using OrleansDashboard;
using Orleans.Runtime.Configuration;

namespace TestHost
{
    internal class OrleansHostWrapper : IDisposable
    {
        public bool Debug
        {
            get { return siloHost != null && siloHost.Debug; }
            set { siloHost.Debug = value; }
        }

        private SiloHost siloHost;


        public OrleansHostWrapper(bool primary)
        {
            if (primary)
            {
                siloHost = new SiloHost("primary", ClusterConfiguration.LocalhostPrimarySilo());
            }
            else
            {
                var rando = new Random();
                var config = new ClusterConfiguration();
                var siloAddress = new IPEndPoint(IPAddress.Loopback, 22222);
                config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.MembershipTableGrain;
                config.Globals.SeedNodes.Add(siloAddress);
                config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;
                config.Defaults.HostNameOrIPAddress = "localhost";
                config.Defaults.Port = rando.Next(50000);
                config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Loopback, rando.Next(50000));
                config.PrimaryNode = siloAddress;
                siloHost = new SiloHost("secondary", config);
            }
        }

        public bool Run()
        {
            bool ok = false;

            try
            {
                siloHost.InitializeOrleansSilo();

                siloHost.Config.Globals.RegisterBootstrapProvider<Dashboard>("Dashboard");

                ok = siloHost.StartOrleansSilo();
                if (!ok) throw new SystemException(string.Format("Failed to start Orleans silo '{0}' as a {1} node.", siloHost.Name, siloHost.Type));
            }
            catch (Exception exc)
            {
                siloHost.ReportStartupError(exc);
                var msg = string.Format("{0}:\n{1}\n{2}", exc.GetType().FullName, exc.Message, exc.StackTrace);
                Console.WriteLine(msg);
            }

            return ok;
        }

        public bool Stop()
        {
            bool ok = false;

            try
            {
                siloHost.StopOrleansSilo();
            }
            catch (Exception exc)
            {
                siloHost.ReportStartupError(exc);
                var msg = string.Format("{0}:\n{1}\n{2}", exc.GetType().FullName, exc.Message, exc.StackTrace);
                Console.WriteLine(msg);
            }

            return ok;
        }

   



        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            siloHost.Dispose();
            siloHost = null;
        }
    }
}
