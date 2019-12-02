using Core.Models.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Core.Pipeline.Interfaces
{
    public interface IMessageDispatcher
    {
        Task DispatchAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> connectedClients);
    }
}
