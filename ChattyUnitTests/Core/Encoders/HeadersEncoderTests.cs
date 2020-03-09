
using Core.Models.Consts;
using Core.Services.Encoders;
using Core.Services.Encoders.Interfaces;
using System.Collections.Generic;
using System.Text;
using Utils;
using Xunit;

namespace ChattyUnitTests.Core.Encoders
{
    public class HeadersEncoderTests
    {
        private readonly IHeadersEncoder _headersEncoder;

        private readonly IDictionary<string, string> _headers;
        private readonly byte[] _headersEncoded;

        public HeadersEncoderTests()
        {
            _headersEncoder = new HeadersEncoder();

            _headers = new Dictionary<string, string>
            {
                { MessageHeaders.Sender, "pokrasa"},
                { MessageHeaders.Recipient, "admin"}
            };

            _headersEncoded = Encoding.ASCII.GetBytes($"{MessageHeaders.Sender}:pokrasa\n{MessageHeaders.Recipient}:admin\n");
        }

        [Fact]
        public void EncodeHeadersTest()
        {
            //Act
            byte[] result = _headersEncoder.Encode(_headers);

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(_headersEncoded.Length, result.Length);
            Assert.True(Compare.EachElement(_headersEncoded, result));
        }

        [Fact]
        public void DecodeHeadersTest()
        {
            //Act
            IDictionary<string, string> result = _headersEncoder.Decode(_headersEncoded);

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey(MessageHeaders.Sender));
            Assert.True(result.ContainsKey(MessageHeaders.Recipient));
            Assert.Equal(_headers[MessageHeaders.Sender], result[MessageHeaders.Sender]);
            Assert.Equal(_headers[MessageHeaders.Recipient], result[MessageHeaders.Recipient]);
        }
    }
}
