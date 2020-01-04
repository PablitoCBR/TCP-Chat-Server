using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.ExceptionHandlers.Interfaces
{

    public interface IExceptionHandler<TException> : IExceptionHandler where TException : Exception
    {
        Task HandleExceptionAsync(TException exception, Socket socket, CancellationToken cancellationToken);
    }
}
