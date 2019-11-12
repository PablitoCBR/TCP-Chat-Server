using AutoMapper;
using Host.Abstractions;
using Host.Builder.Interfaces;
using Host.Builder.Models;
using Host.Listeners;
using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Host.Builder
{
    public class HostBuilder : IHostBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<IHostBuilder> _logger;

        private readonly IListenerFabric _listenerFabric;

        private readonly HostBuilderSettings _builderSettings;

        private readonly IMapper _mapper;

        public HostBuilder(ILogger<IHostBuilder> logger, IOptions<HostBuilderSettings> builderSettings, IListenerFabric listenerFabric, 
            IServiceProvider serviceProvider, IMapper mapper)
        {
            this._logger = logger;
            this._builderSettings = builderSettings.Value;
            this._listenerFabric = listenerFabric;
            this._serviceProvider = serviceProvider;
            this._mapper = mapper;
        }

        public IHost Build()
        {
            IListener tcpListener = this._listenerFabric.CreateTcpListener(this._builderSettings.Port, this._mapper.Map<ListennerSettings>(this._builderSettings));
            return new Host(tcpListener, this._serviceProvider.GetService<ILogger<IHost>>());
        }
    }
}
