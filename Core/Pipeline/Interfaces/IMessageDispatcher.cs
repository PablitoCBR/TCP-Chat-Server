using Core.Models.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core.Pipeline.Interfaces
{
    public interface IMessageDispatcher
    {
        Task DispatchAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> connectedClients);

        Task OnExceptionAsync(IClientInfo clientInfo, Exception exception);

        Task OnExceptionAsync(Socket clientSocket, Exception exception);
    }
}
