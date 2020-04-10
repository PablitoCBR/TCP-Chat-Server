using Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services.Encoders
{
    public class MessageEncoder : IMessageEncoder
    {
        private IHeadersEncoder HeadersEncoder { get; }

        private IFrameMetaEncoder FrameMetaEncoder { get; }

        public MessageEncoder(IHeadersEncoder headersEncoder, IFrameMetaEncoder frameMetaEncoder)
        {
            this.HeadersEncoder = headersEncoder;
            this.FrameMetaEncoder = frameMetaEncoder;
        }

        public Message Decode(byte[] message, FrameMetaData frameMetaData, ClientInfo clientInfo)
        {
            byte[] headersData = message.Take(frameMetaData.HeadersDataLength).ToArray();
            IDictionary<string, string> headers = this.HeadersEncoder.Decode(headersData);
            return new Message(clientInfo, frameMetaData, headers, message.Skip(frameMetaData.HeadersDataLength).ToArray());
        }

        public byte[] Encode(Message message)
        {
            byte[] frameMetaData = this.FrameMetaEncoder.Encode(message.FrameMetaData);
            byte[] headersData = this.HeadersEncoder.Encode(message.Headers);
            List<byte> messageData = new List<byte>();
            messageData.AddRange(frameMetaData);
            messageData.AddRange(headersData);
            messageData.AddRange(message.MessageData);
            return messageData.ToArray();
        }
    }

    public interface IMessageEncoder
    {
        byte[] Encode(Message message);

        Message Decode(byte[] message, FrameMetaData frameMetaData, ClientInfo clientInfo);
    }
}
