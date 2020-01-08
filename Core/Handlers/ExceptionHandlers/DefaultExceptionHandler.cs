namespace Core.Handlers.ExceptionHandlers
{
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Core.Handlers.ExceptionHandlers.Interfaces;
    using Core.Models.Exceptions;
    using Core.Services.Factories.Interfaces;

    public class DefaultExceptionHandler : IExceptionHandler<AbstractException>
    {
        private readonly IMessageFactory _messageFactory;

        public DefaultExceptionHandler(IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
        }

        public async Task HandleExceptionAsync(AbstractException exception, Socket socket, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) 
                return;

            byte[] errorResponse = _messageFactory.CreateBytes(exception.ResponseMessageType, exception.ResponseHeaders, exception.Message);
            await socket.SendAsync(new ArraySegment<byte>(errorResponse), SocketFlags.None, cancellationToken);
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as AbstractException, socket, cancellationToken);
    }
}
