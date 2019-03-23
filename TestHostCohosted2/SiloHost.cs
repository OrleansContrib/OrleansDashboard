using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace TestHostCohosted2
{
    public sealed class SiloHost : IHostedService
    {
        private ISiloHost siloHost;

        public SiloHost(ISiloHost siloHost)
        {
            this.siloHost = siloHost;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await siloHost.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return siloHost.StopAsync();
        }
    }
}
