namespace Core.Handlers.MessageHandlers.Interfaces
{
    using Core.Models;
    using Core.Models.Enums;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageHandler
    {
        MessageType MessageType { get; }

        Task HandleAsync(Message message, ConcurrentDictionary<string, ClientInfo> activeClients, CancellationToken cancellationToken);
    }
}
