using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.ExceptionHandlers.Interfaces;

using Core.Models.Exceptions.ServerExceptions;

using Core.Services.Factories.Interfaces;

namespace Core.Handlers.ExceptionHandlers
{
    public class ClientUnreachableExceptionHandler : IExceptionHandler<ClientUnreachableException>
    {
        private readonly IMessageFactory _messageFactory;

        public ClientUnreachableExceptionHandler(IMessageFactory messageFactory) 
        {
            _messageFactory = messageFactory;
        }

        public async Task HandleExceptionAsync(ClientUnreachableException exception, Socket socket, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            byte[] message = _messageFactory.CreateBytes(exception.ResponseMessageType, exception.Message);
            await socket.SendAsync(new ArraySegment<byte>(message), SocketFlags.None, cancellationToken);
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as ClientUnreachableException, socket, cancellationToken);
    }
}
