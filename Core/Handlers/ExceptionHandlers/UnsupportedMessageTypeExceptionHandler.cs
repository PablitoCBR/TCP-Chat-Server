using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Models.Exceptions.ServerExceptions;
using Core.Services.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.ExceptionHandlers
{
    public class UnsupportedMessageTypeExceptionHandler : IExceptionHandler<UnsupportedMessageTypeException>
    {
        private readonly ILogger<IExceptionHandler> _logger;

        private readonly IMessageFactory _messageFactory;

        public UnsupportedMessageTypeExceptionHandler(ILogger<IExceptionHandler> logger, IMessageFactory messageFactory)
        {
            _logger = logger;
            _messageFactory = messageFactory;
        }

        public async Task HandleExceptionAsync(UnsupportedMessageTypeException exception, Socket socket, CancellationToken cancellationToken)
        {
            _logger.LogError(
                $"Server recived unsupported message type. Message code: {exception.UsedMessageTypeCode}. {Environment.NewLine}" +
                $"Source: {((IPEndPoint)socket?.RemoteEndPoint)?.Address.ToString()} {Environment.NewLine}" + 
                $"Exception message: {exception.Message}");

            byte[] errorResponse = _messageFactory.CreateBytes(exception.ResponseMessageType, exception.ResponseHeaders, exception.Message);
            await socket.SendAsync(new ArraySegment<byte>(errorResponse), SocketFlags.None, cancellationToken);
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as UnsupportedMessageTypeException, socket, cancellationToken);
    }
}
