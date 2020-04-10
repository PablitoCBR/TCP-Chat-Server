using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models.Enums;
using Core.Models.Interfaces;
using Core.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Xunit;

namespace ChattyUnitTests.Core.Pipeline
{
    public class MessageDispatcherTests
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly Mock<IMessageHandler> _messageHandlerMock;
        private readonly Mock<IExceptionHandler<MockedException>> _exceptionHandlerMock;

        private readonly Mock<IMessage> _messageMock;
        private readonly Mock<IFrameMetaData> _frameMetaDataMock;
        private readonly Mock<IClientInfo> _clientInfoMock;

        public MessageDispatcherTests()
        {
            _messageHandlerMock = new Mock<IMessageHandler>();
            _exceptionHandlerMock = new Mock<IExceptionHandler<MockedException>>();
            _frameMetaDataMock = new Mock<IFrameMetaData>();
            _clientInfoMock = new Mock<IClientInfo>();
            _messageMock = new Mock<IMessage>();
            _messageMock.SetupGet(mock => mock.FrameMetaData).Returns(_frameMetaDataMock.Object);
            _messageMock.SetupGet(mock => mock.ClientInfo).Returns(_clientInfoMock.Object);

            var serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddTransient(x => _messageHandlerMock.Object);
            serviceDescriptors.AddTransient(x => _exceptionHandlerMock.Object);

            _messageDispatcher = new MessageDispatcher(serviceDescriptors.BuildServiceProvider(), Mock.Of<ILogger<IMessageDispatcher>>());
        }

        [Fact]
        public async Task DispatchMessageTest()
        {
            // Arrange
            _frameMetaDataMock.SetupGet(mock => mock.Type).Returns(MessageType.MessageSendRequest);
            _messageHandlerMock.SetupGet(mock => mock.MessageType).Returns(_frameMetaDataMock.Object.Type);
            _messageHandlerMock.Setup(
                mock => mock.HandleAsync(_messageMock.Object, It.IsAny<ConcurrentDictionary<string, IClientInfo>>(), It.IsAny<CancellationToken>())
                ).Returns(Task.CompletedTask);

            // Act
            await _messageDispatcher.DispatchAsync(_messageMock.Object, new ConcurrentDictionary<string, IClientInfo>());

            // Assert
            _messageHandlerMock.Verify(
                mock => mock.HandleAsync(_messageMock.Object, It.IsAny<ConcurrentDictionary<string, IClientInfo>>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task DispatchMessageWithHandlerExceptionMock()
        {
            // Arrange
            var socket = new Socket(SocketType.Stream, ProtocolType.Unspecified);
            _clientInfoMock.SetupGet(mock => mock.Socket).Returns(socket);
            _frameMetaDataMock.SetupGet(mock => mock.Type).Returns(MessageType.MessageSendRequest);
            _messageHandlerMock.SetupGet(mock => mock.MessageType).Returns(_frameMetaDataMock.Object.Type);
            _messageHandlerMock.Setup(
                mock => mock.HandleAsync(_messageMock.Object, It.IsAny<ConcurrentDictionary<string, IClientInfo>>(), It.IsAny<CancellationToken>())
                ).Throws<MockedException>();

            // Act
            await _messageDispatcher.DispatchAsync(_messageMock.Object, new ConcurrentDictionary<string, IClientInfo>());

            // Assert
            _exceptionHandlerMock.Verify(
                mock => mock.HandleExceptionAsync(It.IsAny<Exception>(), socket, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task OnExceptionAsyncPassingClientInfoTest()
        {
            // Arrange
            var socket = new Socket(SocketType.Stream, ProtocolType.Unspecified);
            _clientInfoMock.SetupGet(mock => mock.Socket).Returns(socket);

            // Act
            await _messageDispatcher.OnExceptionAsync(_clientInfoMock.Object, new MockedException());

            // Assert
            _exceptionHandlerMock.Verify(
                mock => mock.HandleExceptionAsync(It.IsAny<Exception>(), socket, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task OnExceptionAsyncPassingSocketTest()
        {
            // Arrange
            var socket = new Socket(SocketType.Stream, ProtocolType.Unspecified);

            // Act
            await _messageDispatcher.OnExceptionAsync(socket, new MockedException());

            // Assert
            _exceptionHandlerMock.Verify(
                mock => mock.HandleExceptionAsync(It.IsAny<Exception>(), socket, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
