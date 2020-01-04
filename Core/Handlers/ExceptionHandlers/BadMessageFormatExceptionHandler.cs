using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Models.Exceptions.UserFaultExceptions;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.ExceptionHandlers
{
    class BadMessageFormatExceptionHandler : IExceptionHandler<BadMessageFormatException>
    {
        public Task HandleExceptionAsync(BadMessageFormatException exception, Socket socket, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as BadMessageFormatException, socket, cancellationToken);
    }
}
