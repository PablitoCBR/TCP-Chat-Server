namespace Core.Pipeline.Interfaces
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Core.Models.Interfaces;

    public interface IMessageDispatcher
    {
        Task DispatchAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> connectedClients, CancellationToken cancellationToken);

        Task OnExceptionAsync(IClientInfo clientInfo, Exception exception, CancellationToken cancellationToken);

        Task OnExceptionAsync(Socket clientSocket, Exception exception, CancellationToken cancellationToken);
    }
}
