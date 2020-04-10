using Core.Models;
using Core.Models.Consts;
using Core.Models.Enums;
using Core.Services.Encoders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Xunit;

namespace ChattyFunctionalTests
{
    public class MessageEncoderTests
    {
        private readonly IMessageEncoder _messageEncoder;

        public MessageEncoderTests()
        {
            IOptions<FrameMetaDataConfiguration> options = Options.Create(
                new FrameMetaDataConfiguration()
                {
                    HeadersLengthFieldSize = 4,
                    MessageLengthFieldSize = 4,
                    MetaDataFieldsTotalSize = 9
                });

            _messageEncoder = new MessageEncoder(new HeadersEncoder(), new FrameMetaEncoder(options));
        }

        [Theory]
        [InlineData(MessageType.ActiveUsers)]
        [InlineData(MessageType.MessageSendRequest)]
        [InlineData(MessageType.MessageSent)]
        [InlineData(MessageType.Unauthenticated)]
        [InlineData(MessageType.Registered)]
        [InlineData(MessageType.DHKeyExchangeStepRequest)]
        public void EncodeDiffrentMessageTypesTest(MessageType messageType)
        {
            // Arrange
            Message message = new Message(null, new FrameMetaData(messageType, 0, 0), null, Array.Empty<byte>());

            // Act
            byte[] result = _messageEncoder.Encode(message);

            // Assert
            Assert.Equal((byte)messageType, result[0]);
        }

        [Fact]
        public void EncodeMessageHeadersTest()
        {
            // Arrange
            var headers = new Dictionary<string, string>
            {
                { MessageHeaders.Recipient, "admin" }
            };

            string headersString = $"{MessageHeaders.Recipient}:admin";
            byte[] messageExpectedBytes = Encoding.ASCII.GetBytes(headersString);

            var message = new Message(null, new FrameMetaData(MessageType.MessageSendRequest, 0, messageExpectedBytes.Length), headers, Array.Empty<byte>());

            // Act
            byte[] result = _messageEncoder.Encode(message);

            // Assert
            Assert.True(Compare.EachElement(messageExpectedBytes, result.Skip(9).Take(messageExpectedBytes.Length)));
        }

        [Fact]
        public void DecodeMessageTest()
        {
            // Arrange
            string headersString = $"{MessageHeaders.Recipient}:admin";
            byte[] headersExpectedBytes = Encoding.ASCII.GetBytes(headersString);
            IEnumerable<byte> messageData = Enumerable.Repeat<byte>((byte)1, 10);
            var messageBytes = new List<byte>(headersExpectedBytes);
            messageBytes.AddRange(messageData);

            var frameMetaData = new FrameMetaData(MessageType.MessageSendRequest, headersExpectedBytes.Length, messageData.Count());

            // Act
            Message result = _messageEncoder.Decode(messageBytes.ToArray(), frameMetaData, null);

            // Assert
            Assert.Equal(frameMetaData.Type, result.FrameMetaData.Type);
            Assert.True(result.Headers.ContainsKey(MessageHeaders.Recipient));
            Assert.Equal("admin", result.Headers[MessageHeaders.Recipient]);
            Assert.True(Compare.EachElement(messageData, result.MessageData));
        }
    }
}
