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
    public class SendRequestMessageHandler : IMessageHandler
    {
        private readonly IMessageFactory _messageFactory;

        public SendRequestMessageHandler(IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
        }

        public MessageType MessageType => MessageType.MessageSendRequest;

        public async Task HandleAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> activeClients, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!message.Headers.TryGetValue(MessageHeaders.Recipient, out string recipientName))
                throw new BadMessageFormatException(MessageType.MissingHeader, $"{MessageHeaders.Recipient} header was missing.");

            IClientInfo recipient = activeClients.TryGetValue(recipientName, out IClientInfo result)
                ? result
                : throw new ClientUnreachableException(message.Headers[MessageHeaders.Recipient], MessageType.ClientUnreachable, $"{recipientName} was unreachable.");

            await this.SendMessageAsync(message, recipient.Socket, cancellationToken);
            await this.SendConfirmation(message, cancellationToken);
        }

        private async Task SendMessageAsync(IMessage requestMessage, Socket recipient, CancellationToken cancellationToken)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                {MessageHeaders.Sender, requestMessage.ClientInfo.Name}
            };

            if (requestMessage.Headers.TryGetValue(MessageHeaders.Encryption, out string encryption))
                headers.Add(MessageHeaders.Encryption, encryption);

            if (requestMessage.Headers.TryGetValue(MessageHeaders.MessageGuid, out string guid))
                headers.Add(MessageHeaders.MessageGuid, guid);

            byte[] messageData = _messageFactory.CreateBytes(MessageType.Message, headers, requestMessage.MessageData);
            await recipient.SendAsync(new ArraySegment<byte>(messageData), SocketFlags.None, cancellationToken);
        }

        private async Task SendConfirmation(IMessage requestMessage, CancellationToken cancellationToken)
        {
            byte[] messageData = _messageFactory.CreateBytes(MessageType.MessageSent, requestMessage.Headers);
            await requestMessage.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(messageData), SocketFlags.None, cancellationToken);
        }
    }
}
