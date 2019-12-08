using Core.Models;
using Core.Models.Enums;
using Core.Models.Exceptions;
using Core.Models.Interfaces;
using Core.Services.Encoders.Interfaces;

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services.Encoders
{
    public class FrameMetaEncoder : IFrameMetaEncoder
    {
        private readonly FrameMetaDataConfiguration _configuration;

        public FrameMetaEncoder(IOptions<FrameMetaDataConfiguration> options)
        {
            this._configuration = options.Value;
        }

        public IFrameMetaData Decode(byte[] frameMetaData)
            => new FrameMetaData(
                this.GetMessageType(frameMetaData),
                this.GetSenderId(frameMetaData),
                this.GetHeadersDataLength(frameMetaData),
                this.GetMessageDataLength(frameMetaData)
                );

        public byte[] Encode(IFrameMetaData frameMetaData)
        {
            List<byte> encodedMetaData = new List<byte>() { (byte)frameMetaData.Type };
            encodedMetaData.AddRange(BitConverter.GetBytes(frameMetaData.SenderID));
            encodedMetaData.AddRange(BitConverter.GetBytes(frameMetaData.HeadersDataLength));
            encodedMetaData.AddRange(BitConverter.GetBytes(frameMetaData.MessageDataLength));
            return encodedMetaData.ToArray();
        }

        public int GetHeadersDataLength(byte[] frameMetaData)
        {
            int numberOfBytesBefore = 1 + this._configuration.SenderIdLength;
            IEnumerable<byte> headersLengthBytes = frameMetaData.Skip(numberOfBytesBefore).Take(this._configuration.HeadersDataLength);
            return BitConverter.ToInt32(headersLengthBytes.ToArray());
        }

        public int GetMessageDataLength(byte[] frameMetaData)
        {
            int numberOfBytesBefore = 1 + this._configuration.SenderIdLength + this._configuration.HeadersDataLength;
            IEnumerable<byte> messageLengthBytes = frameMetaData.Skip(numberOfBytesBefore).Take(this._configuration.MessageDataLength);
            return BitConverter.ToInt32(messageLengthBytes.ToArray());
        }

        public MessageType GetMessageType(byte[] frameMetaData)
        {
            if (!Enum.IsDefined(typeof(MessageType), frameMetaData[0]) || (MessageType)frameMetaData[0] == MessageType.None)
                throw new UnsupportedMessageTypeException(frameMetaData[0], "Message frame mata data was containing unrecognized message type code.");
            return (MessageType)frameMetaData[0];
        }

        public int GetSenderId(byte[] frameMetaData)
        {
            IEnumerable<byte> senderIdByte = frameMetaData.Skip(1).Take(this._configuration.SenderIdLength);
            return BitConverter.ToInt32(senderIdByte.ToArray());
        }
    }
}
