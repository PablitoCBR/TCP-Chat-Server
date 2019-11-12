using System;

namespace Core.Models.Exceptions
{
    public class InvalidFrameFormatException : Exception
    {
        public int FrameMetaDataLength { get; }
        public byte[] FrameMetaData { get; }

        public InvalidFrameFormatException(int frameMetaDataLength, byte[] frameMetaData, string message = "") : base(message)
        {
            FrameMetaDataLength = frameMetaDataLength;
            FrameMetaData = frameMetaData;
        }
    }
}
