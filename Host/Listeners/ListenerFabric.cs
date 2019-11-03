using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace Host.Listeners
{
    public class ListenerFabric : IListenerFabric
    {
        private IServiceProvider ServiceProvider { get; }

        public ListenerFabric(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IListener CreateTcpListener(int port, int pendingConnectionsLimit)
        {
            IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            return new TcpListener(pendingConnectionsLimit, ipEndPoint, ServiceProvider.GetService<ILogger<IListener>>());
        }
    }
}
