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
using Core.Services.Security.Interfaces;

namespace Core.Handlers.MessageHandlers
{
    public class DHKeyExchangeRequestMessageHandler : IMessageHandler
    {
        private readonly IPrimeNumberGenerator _primeNumberGenerator;

        private readonly IMessageFactory _messageFactory;

        public DHKeyExchangeRequestMessageHandler(IPrimeNumberGenerator primeNumberGenerator, IMessageFactory messageFactory)
        {
            _primeNumberGenerator = primeNumberGenerator;
            _messageFactory = messageFactory;
        }

        public MessageType MessageType => MessageType.DHKeyExchangeRequest;

        public async Task HandleAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> activeClients, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!message.Headers.TryGetValue(MessageHeaders.Recipient, out string recipientName))
                throw new BadMessageFormatException(MessageType.MissingHeader, $"{MessageHeaders.Recipient} header was missing.");

            IClientInfo recipient = activeClients.TryGetValue(recipientName, out IClientInfo result)
                ? result
                : throw new ClientUnreachableException(message.Headers[MessageHeaders.Recipient], MessageType.ClientUnreachable, $"{recipientName} was unreachable.");

            int p = _primeNumberGenerator.GetRandomPrimeNumber();
            int g = _primeNumberGenerator.GetRandomPrimeNumber();

            IDictionary<string, string> messageHeaders = new Dictionary<string, string>
            {
                { MessageHeaders.MessageGuid, Guid.NewGuid().ToString() },
                { MessageHeaders.DHKey, String.Format("{0}:{1}", p, g) },
                { MessageHeaders.Sender, message.ClientInfo.Name }
            };

            byte[] messageResponse = _messageFactory.CreateBytes(MessageType.DHKeyExchangeInit, messageHeaders);

            await recipient.Socket.SendAsync(new ArraySegment<byte>(messageResponse), SocketFlags.None, cancellationToken);
            await message.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(messageResponse), SocketFlags.None, cancellationToken);
        }
    }
}
