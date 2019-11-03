using Host.Abstractions;
using Host.Builder.Interfaces;
using Host.Builder.Models;
using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Host.Builder
{
    public class HostBuilder : IHostBuilder
    {
        private IServiceProvider ServiceProvider { get; }

        private ILogger<IHostBuilder> Logger { get; }

        private IListenerFabric ListenerFabric { get; }

        private HostBuilderSettings BuilderSettings { get; }

        public HostBuilder(ILogger<IHostBuilder> logger, IOptions<HostBuilderSettings> builderSettings, IListenerFabric listenerFabric, IServiceProvider serviceProvider)
        {
            Logger = logger;
            BuilderSettings = builderSettings.Value;
            ListenerFabric = listenerFabric;
            ServiceProvider = serviceProvider;
        }

        public IHost Build()
        {
            return new Host(ListenerFabric.CreateTcpListener(BuilderSettings.Port, BuilderSettings.PendingConnectionsQueue));
        }
    }
}
