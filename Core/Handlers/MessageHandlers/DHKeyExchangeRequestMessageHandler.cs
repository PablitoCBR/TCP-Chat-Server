using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models;
using Core.Models.Consts;
using Core.Models.Enums;
using Core.Models.Exceptions.ServerExceptions;
using Core.Models.Exceptions.UserFaultExceptions;
using Core.Services.Factories;
using Core.Services.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task HandleAsync(Message message, ConcurrentDictionary<string, ClientInfo> activeClients, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!message.Headers.TryGetValue(MessageHeaders.Recipient, out string recipientName))
                throw new BadMessageFormatException(MessageType.MissingHeader, $"{MessageHeaders.Recipient} header was missing.");

            ClientInfo recipient = activeClients.TryGetValue(recipientName, out ClientInfo result)
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

            cancellationToken.ThrowIfCancellationRequested();
            Task recipientSendingTask = recipient.Socket.SendAsync(new ArraySegment<byte>(messageResponse), SocketFlags.None);
            Task senderSendingTask = message.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(messageResponse), SocketFlags.None);
            await Task.WhenAll(recipientSendingTask, senderSendingTask);
        }
    }
}
