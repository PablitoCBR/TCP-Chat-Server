using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.MessageHandlers.Interfaces;

using Core.Models.Consts;
using Core.Models.Enums;
using Core.Models.Exceptions.ServerExceptions;
using Core.Models.Exceptions.UserFaultExceptions;
using Core.Models.Interfaces;

using Core.Services.Factories.Interfaces;

namespace Core.Handlers.MessageHandlers
{
    public class DHKeyExchangeStepMessageHandler : IMessageHandler
    {
        private readonly IMessageFactory _messageFactory;

        public DHKeyExchangeStepMessageHandler(IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
        }

        public MessageType MessageType => MessageType.DHKeyExchangeStepRequest;

        public async Task HandleAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> activeClients, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!message.Headers.TryGetValue(MessageHeaders.Recipient, out string recipientName))
                throw new BadMessageFormatException(MessageType.MissingHeader, $"{MessageHeaders.Recipient} header was missing.");

            if(!message.Headers.TryGetValue(MessageHeaders.DHKey, out string dhKey))
                throw new BadMessageFormatException(MessageType.MissingHeader, $"{MessageHeaders.DHKey} header was missing.");

            if(!message.Headers.TryGetValue(MessageHeaders.MessageGuid, out string messageGuid))
                throw new BadMessageFormatException(MessageType.MissingHeader, $"{MessageHeaders.MessageGuid} header was missing.");

            IClientInfo recipient = activeClients.TryGetValue(recipientName, out IClientInfo result)
                ? result
                : throw new ClientUnreachableException(message.Headers[MessageHeaders.Recipient], MessageType.ClientUnreachable, $"{recipientName} was unreachable.");

            IDictionary<string, string> messageHeaders = new Dictionary<string, string>
            {
                { MessageHeaders.Recipient, recipient.Name },
                { MessageHeaders.MessageGuid, messageGuid },
                { MessageHeaders.DHKey, dhKey }
            };

            byte[] response = _messageFactory.CreateBytes(MessageType.DHKeyExchange, messageHeaders);
            await recipient.Socket.SendAsync(new ArraySegment<byte>(response), SocketFlags.None, cancellationToken);
            await message.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(_messageFactory.CreateBytes(MessageType.MessageSent)), SocketFlags.None, cancellationToken);
        }
    }
}
