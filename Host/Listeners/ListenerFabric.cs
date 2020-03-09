using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using Microsoft.Extensions.Options;
using Core.Models;

namespace Host.Listeners
{
    public class ListenerFabric : IListenerFabric
    {
        private IServiceProvider ServiceProvider { get; }

        public ListenerFabric(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IListener CreateTcpListener(int port, IOptions<ListenerSettings> settings)
        {
            IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[3];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            return new TcpListener(
                settings.Value, 
                ipEndPoint,
                this.ServiceProvider.GetService<ILogger<IListener>>(), 
                this.ServiceProvider.GetService<IOptions<FrameMetaDataConfiguration>>(),
                this.ServiceProvider);
        }
    }
}
