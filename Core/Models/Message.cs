using System.Collections.Generic;

namespace Core.Models
{
    public class Message 
    {
        public ClientInfo ClientInfo { get; }

        public FrameMetaData FrameMetaData { get; }

        public IDictionary<string, string> Headers { get; }

        public byte[] MessageData { get; }

        public Message(ClientInfo clientInfo, FrameMetaData frameMetaData, IDictionary<string, string> headers,  byte[] messageData)
        {
            ClientInfo = clientInfo;
            MessageData = messageData;
            FrameMetaData = frameMetaData;
            Headers = headers;
        }
    }
}