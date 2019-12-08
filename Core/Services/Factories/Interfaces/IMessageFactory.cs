using Core.Models.Enums;
using Core.Models.Interfaces;
using System.Collections.Generic;

namespace Core.Services.Factories.Interfaces
{
    public interface IMessageFactory
    {
        IMessage Create(IClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message = "");

        byte[] CreateBytes(IClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message = "");

        byte[] CreateBytes(IMessage message);

    }
}
