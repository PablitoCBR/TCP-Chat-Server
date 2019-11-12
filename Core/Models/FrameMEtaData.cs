using Core.Models.Enums;
using Core.Models.Exceptions;
using Core.Models.Interfaces;
using System;

namespace Core.Models
{
    public class FrameMetaData : IFrameMetaData
    {
        public MessageType Type { get; }

        public uint SenderID { get; }

        public uint HeadersDataLength { get; }

        public uint MessageDataLength { get; }

        private FrameMetaData(MessageType type, uint senderId, uint headersDataLength, uint messageDataLength)
        {
            Type = type;
            SenderID = senderId;
            HeadersDataLength = headersDataLength;
            MessageDataLength = messageDataLength;
        }

        public static IFrameMetaData Create(byte[] frameMetaData)
        {
            if (frameMetaData.Length != 13)
                throw new InvalidFrameFormatException(frameMetaData.Length, frameMetaData, "Length of frame meta data was not equal to 17 bytes.");

            if (!Enum.IsDefined(typeof(MessageType), frameMetaData[0]))
                throw new UnsupportedMessageType(frameMetaData[0], "Message frame mata data was containing unrecognized message type code.");

            MessageType type = (MessageType)frameMetaData[0];

            byte[] senderIdBytes = new byte[4] { frameMetaData[1], frameMetaData[2], frameMetaData[3], frameMetaData[4] };
            uint senderId = BitConverter.ToUInt32(senderIdBytes);

            byte[] headersLengthBytes = new byte[] { frameMetaData[5], frameMetaData[6], frameMetaData[7], frameMetaData[8] };
            uint headersLength = BitConverter.ToUInt32(headersLengthBytes);

            byte[] messageLengthBytes = new byte[] { frameMetaData[9], frameMetaData[10], frameMetaData[11], frameMetaData[12] };
            uint messageLength = BitConverter.ToUInt32(messageLengthBytes);

            return new FrameMetaData(type, senderId, headersLength, messageLength);
        }
    }
}
