using System.Collections.Generic;
using System.Text;
using Core.Models;
using Core.Models.Enums;
using Core.Models.Interfaces;
using Core.Services.Encoders.Interfaces;
using Core.Services.Factories.Interfaces;

namespace Core.Services.Factories
{
    public class MessageFactory : IMessageFactory
    {
        private readonly IHeadersEncoder _headersEncoder;

        private readonly IMessageEncoder _messageEncoder;

        public MessageFactory(IHeadersEncoder headersEncoder, IMessageEncoder messageEncoder)
        {
            _headersEncoder = headersEncoder;
            _messageEncoder = messageEncoder;
        }

        public IMessage Create(IClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message)
        {
            byte[] messageData = Encoding.ASCII.GetBytes(message);
            byte[] headersData = _headersEncoder.Encode(headers);
            IFrameMetaData frameMetaData = new FrameMetaData(messageType, 0, headersData.Length, messageData.Length);
            return new Message(clientInfo, frameMetaData, headers, messageData);
        }

        public byte[] CreateBytes(IClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message)
        {
            IMessage messageInstance = this.Create(clientInfo, messageType, headers, message);
            return this.CreateBytes(messageInstance);
        }

        public byte[] CreateBytes(IMessage message) => _messageEncoder.Encode(message);
    }
}
