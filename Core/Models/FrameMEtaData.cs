using Core.Models.Enums;
using Core.Models.Interfaces;

namespace Core.Models
{
    public class FrameMetaData : IFrameMetaData
    {
        public MessageType Type { get; }

        public int SenderID { get; }

        public int HeadersDataLength { get; }

        public int MessageDataLength { get; }

        public FrameMetaData(MessageType type, int senderId, int headersDataLength, int messageDataLength)
        {
            Type = type;
            SenderID = senderId;
            HeadersDataLength = headersDataLength;
            MessageDataLength = messageDataLength;
        }
    }
}
