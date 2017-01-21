using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    internal class Startup
    {
        private IProviderRuntime _providerRuntime;
        private TaskScheduler _taskScheduler;

        public Startup(IHostingEnvironment env)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var dashboardController = new DashboardController(_providerRuntime, _taskScheduler);

            var dashboardRouteHandler = new RouteHandler(context =>
            {
                try
                {
                    var routeValues = context.GetRouteData().Values;
                    switch (routeValues["Action"].ToString().ToUpper())
                    {
                        case "INDEX":
                            return dashboardController.Index(context, routeValues);
                        case "INDEX.MIN.JS":
                            return dashboardController.IndexJs(context, routeValues);
                        case "DASHBOARDCOUNTERS":
                            return dashboardController.GetDashboardCounters(context, routeValues);
                        case "RUNTIMESTATS":
                            return dashboardController.GetRuntimeStats(context, routeValues);
                        case "HISTORICALSTATS":
                            return dashboardController.GetHistoricalStats(context, routeValues);
                        case "GRAINSTATS":
                            return dashboardController.GetGrainStats(context, routeValues);
                        case "SILOPROPERTIES":
                            return dashboardController.GetSiloExtendedProperties(context, routeValues);
                        default:
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return context.Response.WriteAsync("");
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return context.Response.ReturnJson(ex);
                }
            });

            var routeBuilder = new RouteBuilder(app, dashboardRouteHandler);

            routeBuilder.MapRoute(
                name: "default",
                template: "{action=Index}/{id?}");

            app.UseRouter(routeBuilder.Build());
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _providerRuntime = services.Single(x => x.ServiceType == typeof(IProviderRuntime)).ImplementationInstance as IProviderRuntime;
            _taskScheduler = services.Single(x => x.ServiceType == typeof(TaskScheduler)).ImplementationInstance as TaskScheduler;
            services.AddRouting();
        }
    }
}