using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.ExceptionHandlers.Interfaces
{
    public interface IExceptionHandler
    {
        Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken);
    }
}
