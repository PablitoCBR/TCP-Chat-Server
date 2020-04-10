using System;

using Host.Listeners;
using Host.Listeners.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Host.Builder
{
    public class HostBuilder : IHostBuilder
    {
        private int _port = 8000;

        private readonly IServiceProvider _serviceProvider;

        private readonly IListenerFabric _listenerFabric;

        public HostBuilder(IListenerFabric listenerFabric, IServiceProvider serviceProvider)
        {
            _listenerFabric = listenerFabric;
            _serviceProvider = serviceProvider;
        }

        public IHost Build()
        {
            IListener tcpListener = _listenerFabric.CreateTcpListener(_port, _serviceProvider.GetRequiredService<IOptions<ListenerSettings>>());
            return new Host(tcpListener, _serviceProvider.GetService<ILogger<IHost>>());
        }

        public IHostBuilder SetPort(int port)
        {
            _port = port;
            return this;
        }
    }

    public interface IHostBuilder
    {
        IHost Build();
        IHostBuilder SetPort(int port);
    }
}
