using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Core.MessageHandlers.Interfaces;
using Core.Models.Interfaces;
using Core.Pipeline.Interfaces;

namespace Core.Pipeline
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private IEnumerable<IMessageHandler> MessageHandlers { get; }

        public MessageDispatcher(IEnumerable<IMessageHandler> messageHandlers)
        {
            this.MessageHandlers = messageHandlers;
        }

        public async Task DispatchAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> connectedClients)
        {
            IMessageHandler messageHandler = this.MessageHandlers.Single(x => x.MessageType == message.FrameMetaData.Type);
            await messageHandler.HandleAsync(message, connectedClients);
        }
    }
}
