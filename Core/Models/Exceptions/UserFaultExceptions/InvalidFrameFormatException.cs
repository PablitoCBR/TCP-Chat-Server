using Core.Models.Enums;
using Core.Models.Exceptions.UserFaultExceptions;

namespace Core.Models.Exceptions
{
    public class InvalidFrameFormatException : InvalidMessageException
    {
        public int FrameMetaDataLength { get; }
        public byte[] FrameMetaData { get; }

        public InvalidFrameFormatException(int frameMetaDataLength, byte[] frameMetaData, string message = "") : base(MessageType.None, message)
        {
            FrameMetaDataLength = frameMetaDataLength;
            FrameMetaData = frameMetaData;
        }
    }
}
