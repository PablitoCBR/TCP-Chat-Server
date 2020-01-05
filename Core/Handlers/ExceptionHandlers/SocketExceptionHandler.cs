using Core.Handlers.ExceptionHandlers.Interfaces;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.ExceptionHandlers
{
    public class SocketExceptionHandler : IExceptionHandler<SocketException>
    {
        public async Task HandleExceptionAsync(SocketException socketException, Socket socket, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            // socket exception status code trzeba je ogarnac :(

            socket.Disconnect(false);
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as SocketException, socket, cancellationToken);
    }
}
