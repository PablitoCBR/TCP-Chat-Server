using Core.Models.Enums;
using Core.Models.Interfaces;
using System.Collections.Generic;

namespace Core.Models
{
    public class Message : IMessage
    {
        public IClientInfo ClientInfo { get; }

        public IFrameMetaData FrameMetaData { get; }
        
        public IDictionary<string, string> Headers { get; }
        
        public byte[] HeadersData { get; }

        public byte[] MessageData { get; }

        private Message(IClientInfo clientInfo, MessageType type, IFrameMetaData frameMetaData,byte[] headerData, byte[] messageData)
        {
            ClientInfo = clientInfo;
            HeadersData = headerData;
            MessageData = messageData;
            FrameMetaData = frameMetaData;
        }

        public static IMessage Create(IClientInfo clientInfo, MessageType type, IFrameMetaData frameMetaData, byte[] headerData, byte[] messageData)
            => new Message(clientInfo, type, frameMetaData, headerData, messageData);

        public static IMessage Create(IClientInfo clientInfo, IFrameMetaData frameMetaData, byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}
