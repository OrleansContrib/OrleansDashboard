using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Runtime;
using OrleansDashboard.Client;
using OrleansDashboard.Dispatchers;

namespace OrleansDashboard.Hosting
{
    internal class HostingStartupTask : IStartupTask, IDisposable
    {
        private readonly IGrainFactory _grainFactory;
        private readonly HostingOptions _hostingOptions;
        private readonly ILogger<DashboardStartupTask> _logger;
        private readonly SiloDispatcher _siloDispatcher;
        private IWebHost _host;

        public HostingStartupTask(
            ILogger<DashboardStartupTask> logger,
            IGrainFactory grainFactory,
            IOptions<HostingOptions> dashboardOptions,
            SiloDispatcher siloDispatcher)
        {
            _logger = logger;
            _grainFactory = grainFactory;
            _siloDispatcher = siloDispatcher;
            _hostingOptions = dashboardOptions.Value;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            try
            {
                _host =
                    new WebHostBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton(DashboardLogger.Instance);
                            services.AddSingleton(Options.Create(_hostingOptions));
                            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
                            services.AddSingleton<IExternalDispatcher>(_siloDispatcher);
                            services.AddSingleton(_grainFactory);
                        })
                        .Configure(app =>
                        {
                            if (_hostingOptions.HasUsernameAndPassword()) app.UseMiddleware<BasicAuthMiddleware>();

                            if (string.IsNullOrEmpty(_hostingOptions?.BasePath) || _hostingOptions.BasePath == "/")
                            {
                                app.UseMiddleware<DashboardMiddleware>();
                            }
                            else
                            {
                                //Make sure there is a leading slash
                                var basePath = _hostingOptions.BasePath.StartsWith("/")
                                    ? _hostingOptions.BasePath
                                    : "/" + _hostingOptions.BasePath;
                                app.Map(basePath, a => a.UseMiddleware<DashboardMiddleware>());
                            }
                        })
                        .UseKestrel()
                        .UseUrls($"http://{_hostingOptions.Host}:{_hostingOptions.Port}")
                        .Build();

                _host.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(10001, ex.ToString());
            }

            _logger.LogInformation($"Dashboard listening on {_hostingOptions.Port}");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                _host?.Dispose();
            }
            catch
            {
                /* NOOP */
            }
        }
    }
}