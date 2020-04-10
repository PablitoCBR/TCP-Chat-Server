using Core.Models;
using Core.Models.Consts;
using Core.Models.Enums;
using Core.Models.Interfaces;
using Core.Services.Encoders;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Xunit;

namespace ChattyUnitTests.Core.Encoders
{
    public class MessageEncoderTests
    {
        private readonly IMessageEncoder _messageEncoder;

        private readonly Mock<IHeadersEncoder> _headersEncoderMock;
        private readonly Mock<IFrameMetaEncoder> _frameMetaEncoderMock;

        private readonly IMessage _message;
        private readonly IFrameMetaData _frameMetaData;
        private readonly byte[] _messageData;

        private int _frameMetaDataLength = 9;

        public MessageEncoderTests()
        {
            // Create mocks
            _headersEncoderMock = new Mock<IHeadersEncoder>();
            _frameMetaEncoderMock = new Mock<IFrameMetaEncoder>();

            // Configure message headers
            var headers = new Dictionary<string, string>
            {
                { MessageHeaders.Sender, "pokrasa"},
                { MessageHeaders.Recipient, "admin"}
            };
            byte[] headersEncoded = Encoding.ASCII.GetBytes($"{MessageHeaders.Sender}:pokrasa\n{MessageHeaders.Recipient}:admin\n");

            // Setup message frame meta data
            _frameMetaData = new FrameMetaData(MessageType.MessageSendRequest, headersEncoded.Length, 1024);

            // Create message expected message
            var messageBytes = new List<byte> { (byte)_frameMetaData.Type };
            messageBytes.AddRange(BitConverter.GetBytes(_frameMetaData.HeadersDataLength));
            messageBytes.AddRange(BitConverter.GetBytes(_frameMetaData.MessageDataLength));
            messageBytes.AddRange(headersEncoded);
            messageBytes.AddRange(Enumerable.Repeat<byte>(1, _frameMetaData.MessageDataLength));
            _messageData = messageBytes.ToArray();

            // Bind expected message and instance of message encoder
            _message = new Message(null, _frameMetaData, headers, Enumerable.Repeat<byte>(1, _frameMetaData.MessageDataLength).ToArray());
            _messageEncoder = new MessageEncoder(_headersEncoderMock.Object, _frameMetaEncoderMock.Object);
        }

        [Fact]
        public void DecodeMessageTest()
        {
            //Arrange
            byte[] headersBytes = _messageData.Skip(_frameMetaDataLength).Take(_frameMetaData.HeadersDataLength).ToArray();
            _headersEncoderMock
                .Setup(mock => mock.Decode(It.Is<byte[]>(headers => Compare.EachElement(headers, headersBytes))))
                .Returns(_message.Headers);

            //Act
            IMessage result = _messageEncoder.Decode(_messageData.Skip(_frameMetaDataLength).ToArray(), _frameMetaData, null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(_message.FrameMetaData.Type, result.FrameMetaData.Type);
            Assert.Equal(_message.MessageData.Length, result.MessageData.Length);
            Assert.NotNull(result.Headers);
            Assert.Equal(_message.Headers.Count, result.Headers.Count);
            Assert.True(result.Headers.ContainsKey(MessageHeaders.Sender));
        }

        [Fact]
        public void EncodeMessageTest()
        {
            //Arrange
            _frameMetaEncoderMock
                .Setup(mock => mock.Encode(_message.FrameMetaData))
                .Returns(_messageData.Take(_frameMetaDataLength).ToArray());
            _headersEncoderMock
                .Setup(mock => mock.Encode(_message.Headers))
                .Returns(_messageData.Skip(_frameMetaDataLength).Take(_message.FrameMetaData.HeadersDataLength).ToArray());

            //Act
            byte[] result = _messageEncoder.Encode(_message);

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(Compare.EachElement(_messageData, result));
        }
    }
}