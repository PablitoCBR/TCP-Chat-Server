﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models.Enums;
using Core.Models.Interfaces;

namespace Core.Handlers.MessageHandlers
{
    public class DHKeyExchangeStepMessageHandler : IMessageHandler
    {
        public MessageType MessageType => MessageType.DHKeyExchangeStep;

        public Task HandleAsync(IMessage message, ConcurrentDictionary<string, IClientInfo> activeClients, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
