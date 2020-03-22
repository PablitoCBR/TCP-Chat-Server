using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Handlers.MessageHandlers.Interfaces;

using Core.Models.Enums;
using Core.Models.Exceptions;
using Core.Models.Interfaces;

using Core.Pipeline.Interfaces;

using Core.Services.Factories.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Pipeline
{   
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IMessageDispatcher> _logger;

        public MessageDispatcher(IServiceProvider serviceProvider, ILogger<IMessageDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task DispatchAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> connectedClients, CancellationToken cancellationToken)
        {
            try
            {
                IMessageHandler messageHandler = _serviceProvider.GetRequiredService<IEnumerable<IMessageHandler>>().Single(x => x.MessageType == message.FrameMetaData.Type);
                await messageHandler.HandleAsync(message, connectedClients, cancellationToken);
            }
            catch(Exception ex)
            {
                await OnExceptionAsync(message.ClientInfo, ex, cancellationToken);
            }
        }

        public async Task OnExceptionAsync(IClientInfo clientInfo, Exception exception, CancellationToken cancellationToken) 
           => await this.OnExceptionAsync(clientInfo.Socket, exception, cancellationToken);


        public async Task OnExceptionAsync(Socket clientSocket, Exception exception, CancellationToken cancellationToken)  
        {
            if (_serviceProvider.GetService(typeof(IExceptionHandler<>).MakeGenericType(exception.GetType())) is IExceptionHandler exceptionHandler)
                await exceptionHandler.HandleExceptionAsync(exception, clientSocket, cancellationToken);
            else
            {
                if (exception is AbstractException)
                    await _serviceProvider.GetRequiredService<IExceptionHandler<AbstractException>>().HandleExceptionAsync(exception, clientSocket, cancellationToken);
                else
                {
                    Guid unhandledExceptionGuid = Guid.NewGuid();
                    _logger.LogCritical(exception, "{0} Unhandled exception occured. Message: {1}", unhandledExceptionGuid.ToString(), exception.Message);
                    byte[] errorMessage = _serviceProvider.GetRequiredService<IMessageFactory>().CreateBytes(MessageType.InternalServerError, unhandledExceptionGuid.ToString());
                    await clientSocket.SendAsync(new ArraySegment<byte>(errorMessage), SocketFlags.None, cancellationToken);
                }
            }
        }
    }
}
