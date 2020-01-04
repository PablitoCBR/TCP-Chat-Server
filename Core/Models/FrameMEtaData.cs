using Core.Models.Enums;
using Core.Models.Interfaces;

namespace Core.Models
{
    public class FrameMetaData : IFrameMetaData
    {
        public MessageType Type { get; }

        public int HeadersDataLength { get; }

        public int MessageDataLength { get; }

        public FrameMetaData(MessageType type, int headersDataLength, int messageDataLength)
        {
            Type = type;
            HeadersDataLength = headersDataLength;
            MessageDataLength = messageDataLength;
        }
    }
}
