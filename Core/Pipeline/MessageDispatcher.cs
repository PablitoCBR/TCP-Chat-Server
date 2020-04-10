using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Models.Exceptions;
using Core.Services.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Pipeline
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(IServiceProvider serviceProvider, ILogger<MessageDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task DispatchAsync(Message message, ConcurrentDictionary<string, ClientInfo> connectedClients, CancellationToken cancellationToken)
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

        public async Task OnExceptionAsync(ClientInfo clientInfo, Exception exception, CancellationToken cancellationToken) 
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

    public interface IMessageDispatcher
    {
        Task DispatchAsync(Message message, ConcurrentDictionary<string, ClientInfo> connectedClients, CancellationToken cancellationToken = default);

        Task OnExceptionAsync(ClientInfo clientInfo, Exception exception, CancellationToken cancellationToken = default);

        Task OnExceptionAsync(Socket clientSocket, Exception exception, CancellationToken cancellationToken = default);
    }
}
