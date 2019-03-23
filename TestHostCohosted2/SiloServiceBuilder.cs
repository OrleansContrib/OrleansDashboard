using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using HostingBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;
using WebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TestHostCohosted2
{
    public sealed class SiloServiceBuilder : ISiloBuilder
    {
        private readonly HostingBuilderContext context = new HostingBuilderContext(new Dictionary<object, object>());
        private readonly List<Action<HostingBuilderContext, ISiloBuilder>> configureSiloDelegates = new List<Action<HostingBuilderContext, ISiloBuilder>>();
        private readonly List<Action<HostingBuilderContext, IServiceCollection>> configureServicesDelegates = new List<Action<HostingBuilderContext, IServiceCollection>>();

        public IDictionary<object, object> Properties
        {
            get { return context.Properties; }
        }

        public SiloServiceBuilder(IConfiguration config, WebHostEnvironment environment)
        {
            context.Configuration = config;
            context.HostingEnvironment = new EnvironmentWrapper(environment);
        }

        public void Build(IServiceCollection serviceCollection)
        {
            foreach (var configurationDelegate in configureSiloDelegates)
            {
                configurationDelegate(context, this);
            }

            serviceCollection.AddHostedService<SiloHost>();

            this.ConfigureDefaults();
            this.ConfigureApplicationParts(parts => parts.ConfigureDefaults());

            foreach (var configurationDelegate in configureServicesDelegates)
            {
                configurationDelegate(context, serviceCollection);
            }
        }

        public ISiloBuilder ConfigureSilo(Action<HostingBuilderContext, ISiloBuilder> configureDelegate)
        {
            configureSiloDelegates.Add(configureDelegate);

            return this;
        }

        public ISiloBuilder ConfigureServices(Action<HostingBuilderContext, IServiceCollection> configureDelegate)
        {
            configureServicesDelegates.Add(configureDelegate);

            return this;
        }
    }
}
