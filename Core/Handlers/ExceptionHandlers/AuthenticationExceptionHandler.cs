using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Models.Exceptions.UserFaultExceptions;

namespace Core.Handlers.ExceptionHandlers
{
    public class AuthenticationExceptionHandler : IExceptionHandler<AuthenticationException>
    {
        public Task HandleExceptionAsync(AuthenticationException exception, Socket socket, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as AuthenticationException, socket, cancellationToken);
    }
}
