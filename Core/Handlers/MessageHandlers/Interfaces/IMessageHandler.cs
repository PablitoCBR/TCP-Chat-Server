namespace Core.Handlers.MessageHandlers.Interfaces
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Core.Models.Enums;
    using Core.Models.Interfaces;

    public interface IMessageHandler
    {
        MessageType MessageType { get; }

        Task HandleAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> activeClients, CancellationToken cancellationToken);
    }
}
