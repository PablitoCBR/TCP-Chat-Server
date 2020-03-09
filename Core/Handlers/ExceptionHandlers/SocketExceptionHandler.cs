using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.ExceptionHandlers.Interfaces;

using Microsoft.Extensions.Logging;

namespace Core.Handlers.ExceptionHandlers
{
    public class SocketExceptionHandler : IExceptionHandler<SocketException>
    {
        private readonly ILogger<SocketExceptionHandler> _logger;

        public SocketExceptionHandler(ILogger<SocketExceptionHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleExceptionAsync(SocketException socketException, Socket socket, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) 
                return Task.FromCanceled(cancellationToken);

            if (socketException.SocketErrorCode.Equals(SocketError.ConnectionReset))
                _logger.LogWarning(socketException.Message);
            
            socket.Disconnect(false);
            return Task.CompletedTask;
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as SocketException, socket, cancellationToken);
    }
}
