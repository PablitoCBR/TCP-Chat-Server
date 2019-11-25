using Core.Models.Enums;
using Core.Models.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Core.MessageHandlers.Interfaces
{
    public interface IMessageHandler
    {
        MessageType MessageType { get; }

        Task HandleAsync(IMessage message, ConcurrentDictionary<int, IClientInfo> activeClients);
    }
}
