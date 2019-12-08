using Core.Models.Interfaces;
using System.Collections.Generic;

namespace Core.Models
{
    public class Message : IMessage
    {
        public IClientInfo ClientInfo { get; }

        public IFrameMetaData FrameMetaData { get; }

        public IDictionary<string, string> Headers { get; }

        public byte[] MessageData { get; }

        public Message(IClientInfo clientInfo, IFrameMetaData frameMetaData, IDictionary<string, string> headers,  byte[] messageData)
        {
            ClientInfo = clientInfo;
            MessageData = messageData;
            FrameMetaData = frameMetaData;
            Headers = headers;
        }
    }
}