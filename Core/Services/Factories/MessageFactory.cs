using Core.Models;
using Core.Models.Enums;
using Core.Services.Encoders;
using System.Collections.Generic;
using System.Text;

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

        public Message Create(ClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message = "")
        {
            byte[] messageData = Encoding.ASCII.GetBytes(message);
            return this.Create(clientInfo, messageType, headers, messageData);
        }

        public Message Create(ClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, byte[] messageData)
        {
            byte[] headersData = _headersEncoder.Encode(headers);
            FrameMetaData frameMetaData = new FrameMetaData(messageType, headersData.Length, messageData.Length);
            return new Message(clientInfo, frameMetaData, headers, messageData);
        }

        public byte[] CreateBytes(MessageType messageType, IDictionary<string, string> headers, string message = "")
        {
            Message messageInstance = this.Create(null, messageType, headers, message);
            return this.CreateBytes(messageInstance);
        }

        public byte[] CreateBytes(MessageType messageType, IDictionary<string, string> headers, byte[] messageData)
        {
            Message message = this.Create(null, messageType, headers, messageData ?? new byte[0]);
            return this.CreateBytes(message);
        }

        public byte[] CreateBytes(Message message) 
            => _messageEncoder.Encode(message);

        public byte[] CreateBytes(MessageType messageType)
            => this.CreateBytes(messageType, new Dictionary<string, string>());

        public byte[] CreateBytes(MessageType messageType, string message)
            => this.CreateBytes(messageType, new Dictionary<string, string>(), message);
    }

    public interface IMessageFactory
    {
        Message Create(ClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, string message = "");

        Message Create(ClientInfo clientInfo, MessageType messageType, IDictionary<string, string> headers, byte[] messageData);

        byte[] CreateBytes(MessageType messageType);

        byte[] CreateBytes(MessageType messageType, string message);

        byte[] CreateBytes(MessageType messageType, IDictionary<string, string> headers, string message = "");

        byte[] CreateBytes(MessageType messageType, IDictionary<string, string> headers, byte[] messageData);

        byte[] CreateBytes(Message message);
    }
}
