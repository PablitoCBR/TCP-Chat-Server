using System;

using Host.Abstractions;
using Host.Builder.Interfaces;
using Host.Builder.Models;
using Host.Listeners;
using Host.Listeners.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Host.Builder
{
    public class HostBuilder : IHostBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<IHostBuilder> _logger;

        private readonly IListenerFabric _listenerFabric;

        private readonly HostBuilderSettings _builderSettings;

        public HostBuilder(ILogger<IHostBuilder> logger, IOptions<HostBuilderSettings> builderSettings, IListenerFabric listenerFabric, 
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._builderSettings = builderSettings.Value;
            this._listenerFabric = listenerFabric;
            this._serviceProvider = serviceProvider;
        }

        public IHost Build()
        {
            IListener tcpListener = this._listenerFabric.CreateTcpListener(this._builderSettings.Port, _serviceProvider.GetRequiredService<IOptions<ListenerSettings>>());
            return new Host(tcpListener, this._serviceProvider.GetService<ILogger<IHost>>());
        }
    }
}
