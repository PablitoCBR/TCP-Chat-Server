using Core.Models.Enums;
using Core.Models.Interfaces;
using System.Collections.Generic;

namespace Core.Services.Factories.Interfaces
{
    public interface IMessageFactory
    {
        IMessage Create(IClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message = "");

        IMessage Create(IClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, byte[] messageData);

        byte[] CreateBytes(MessageType messageType);

        byte[] CreateBytes(MessageType messageType, IDictionary<string, string> headers, string message = "");

        byte[] CreateBytes(MessageType messageType, IDictionary<string, string> headers, byte[] messageData);

        byte[] CreateBytes(IMessage message);
    }
}
