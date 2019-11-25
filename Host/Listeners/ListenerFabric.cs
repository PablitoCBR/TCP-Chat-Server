using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using Microsoft.Extensions.Options;
using Core.Models;
using Core.Services.Interfaces;
using Core.Pipeline.Interfaces;
using Core.MessageHandlers.Interfaces;
using Core.MessageHandlers;

namespace Host.Listeners
{
    public class ListenerFabric : IListenerFabric
    {
        private IServiceProvider ServiceProvider { get; }

        public ListenerFabric(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IListener CreateTcpListener(int port, ListennerSettings settings)
        {
            IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            return new TcpListener(
                settings, 
                ipEndPoint,
                this.ServiceProvider.GetService<ILogger<IListener>>(), 
                this.ServiceProvider.GetService<IFrameMetaEncoder>(),
                this.ServiceProvider.GetService<IOptions<FrameMetaDataConfiguration>>(),
                this.ServiceProvider.GetService<IMessageEncoder>(),
                this.ServiceProvider.GetService<IAuthenticationHandler>());
        }
    }
}
