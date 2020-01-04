using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models.Enums;
using Core.Models.Interfaces;

namespace Core.Handlers.MessageHandlers
{
    public class DHKeyExchangeRequestMessageHandler : IMessageHandler
    {
        public MessageType MessageType => MessageType.DHKeyExchangeRequest;

        public Task HandleAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> activeClients, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
