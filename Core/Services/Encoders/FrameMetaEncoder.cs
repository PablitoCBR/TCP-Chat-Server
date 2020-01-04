using System;
using System.Collections.Generic;
using System.Linq;

using Core.Models;
using Core.Models.Enums;
using Core.Models.Exceptions.ServerExceptions;
using Core.Models.Interfaces;

using Core.Services.Encoders.Interfaces;

using Microsoft.Extensions.Options;

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
                this.GetHeadersDataLength(frameMetaData),
                this.GetMessageDataLength(frameMetaData)
                );

        public byte[] Encode(IFrameMetaData frameMetaData)
        {
            List<byte> encodedMetaData = new List<byte>() { (byte)frameMetaData.Type };
            encodedMetaData.AddRange(BitConverter.GetBytes(frameMetaData.HeadersDataLength));
            encodedMetaData.AddRange(BitConverter.GetBytes(frameMetaData.MessageDataLength));
            return encodedMetaData.ToArray();
        }

        public int GetHeadersDataLength(byte[] frameMetaData)
        {
            IEnumerable<byte> headersLengthBytes = frameMetaData.Skip(1).Take(_configuration.HeadersLengthFieldSize);
            return BitConverter.ToInt32(headersLengthBytes.ToArray());
        }

        public int GetMessageDataLength(byte[] frameMetaData)
        {
            IEnumerable<byte> messageLengthBytes = frameMetaData
                .Skip(_configuration.MetaDataFieldsTotalSize - _configuration.MessageLengthFieldSize)
                .Take(this._configuration.MessageLengthFieldSize);
            return BitConverter.ToInt32(messageLengthBytes.ToArray());
        }

        public MessageType GetMessageType(byte[] frameMetaData)
        {
            if (!Enum.IsDefined(typeof(MessageType), frameMetaData[0]) || (MessageType)frameMetaData[0] == MessageType.None)
                throw new UnsupportedMessageTypeException(frameMetaData[0], MessageType.UnrecognizedMessageType, "Message frame mata data was containing unrecognized message type code.");
            return (MessageType)frameMetaData[0];
        }
    }
}
