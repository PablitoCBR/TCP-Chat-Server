using Core.Models.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Core.Pipeline.Interfaces
{
    public interface IDispatcher
    {
        Task DispatchAsync(IMessage message, ConcurrentDictionary<int, IClientInfo> connectedClients);
    }
}
