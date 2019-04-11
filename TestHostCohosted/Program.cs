using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace TestHostCohosted
{
    class Program
    {
        static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddServicesForSelfHostedDashboard(null, options =>
                    {
                        options.HideTrace = true;
                    });

                    services.AddSingleton<SiloHost>();
                    services.AddSingleton<IHostedService>(c => c.GetRequiredService<SiloHost>());
                    services.AddSingleton(c => c.GetRequiredService<SiloHost>().GrainFactory);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .Configure(app =>
                {
                    app.UseOrleansDashboard();

                    app.Map("/dashboard", d =>
                    {
                        d.UseOrleansDashboard();
                    });
                })
                .Build()
                .Run();
        }
    }
}
