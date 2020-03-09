using Core.Models;
using Core.Models.Enums;
using Core.Models.Interfaces;
using Core.Services.Encoders;
using Core.Services.Encoders.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using Utils;
using Xunit;

namespace ChattyUnitTests.Core.Encoders
{
    public class FrameMetaEncoderTests
    {
        private readonly IFrameMetaEncoder _frameMetaEncoder;
        private readonly FrameMetaDataConfiguration _configuration;

        private readonly byte[] _testMessageEncoded;
        private readonly IFrameMetaData _testMessageDecoded;

        public FrameMetaEncoderTests()
        {
            _configuration = new FrameMetaDataConfiguration()
            {
                HeadersLengthFieldSize = 4,
                MessageLengthFieldSize = 4,
                MetaDataFieldsTotalSize = 9
            };

            var headersLenth = 50;
            var messageLength = 1024;
            var messageType = MessageType.MessageSendRequest;
            
            _testMessageDecoded = new FrameMetaData(messageType, headersLenth, messageLength);
            var bytes = new List<byte> { (byte)messageType };
            bytes.AddRange(BitConverter.GetBytes(headersLenth));
            bytes.AddRange(BitConverter.GetBytes(messageLength));
            _testMessageEncoded = bytes.ToArray();

            _frameMetaEncoder = new FrameMetaEncoder(Options.Create(_configuration));
        }

        [Fact]
        public void EncodeFrameMetaDataTest()
        {
            //Act
            byte[] result = _frameMetaEncoder.Encode(_testMessageDecoded);

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(_testMessageEncoded.Length, result.Length);
            Assert.True(Compare.EachElement(_testMessageEncoded, result));
        }

        [Fact]
        public void DecodeFrameMetaDataTest()
        {
            //Act
            IFrameMetaData result = _frameMetaEncoder.Decode(_testMessageEncoded);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(_testMessageDecoded.Type, result.Type);
            Assert.Equal(_testMessageDecoded.HeadersDataLength, result.HeadersDataLength);
            Assert.Equal(_testMessageDecoded.MessageDataLength, result.MessageDataLength);
        }

        [Fact]
        public void GetMessageTypeTest()
        {
            //Act
            MessageType result = _frameMetaEncoder.GetMessageType(_testMessageEncoded);

            //Assert
            Assert.Equal(_testMessageDecoded.Type, result);
        }

        [Fact]
        public void GetMessageDataLengthTest()
        {
            //Act
            int result = _frameMetaEncoder.GetMessageDataLength(_testMessageEncoded);

            //Assert
            Assert.Equal(_testMessageDecoded.MessageDataLength, result);
        }

        [Fact]
        public void GetHeadersDataLengthTest()
        {
            //Act
            int result = _frameMetaEncoder.GetHeadersDataLength(_testMessageEncoded);

            //Assert
            Assert.Equal(_testMessageDecoded.HeadersDataLength, result);
        }
    }
}