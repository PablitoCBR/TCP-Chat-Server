using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Services.Factories;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.MessageHandlers
{
    public class ActiveUsersUpdateRequestMessageHandler : IMessageHandler
    {
        private readonly IMessageFactory _messageFactory;

        public ActiveUsersUpdateRequestMessageHandler(IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
        }

        public MessageType MessageType => MessageType.ActiveUsersUpdataRequest;

        public async Task HandleAsync(Message message, ConcurrentDictionary<string, ClientInfo> activeClients, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] response = _messageFactory.CreateBytes(MessageType.ActiveUsers, String.Join(',', activeClients.Keys.ToArray()));
            await message.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(response), SocketFlags.None, cancellationToken);
        }
    }
}
