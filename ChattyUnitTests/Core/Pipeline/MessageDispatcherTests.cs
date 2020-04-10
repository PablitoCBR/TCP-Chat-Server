using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Handlers.MessageHandlers.Interfaces;
using Core.Models;
using Core.Models.Enums;
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

        public MessageDispatcherTests()
        {
            _messageHandlerMock = new Mock<IMessageHandler>();
            _exceptionHandlerMock = new Mock<IExceptionHandler<MockedException>>();

            var serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddTransient(x => _messageHandlerMock.Object);
            serviceDescriptors.AddTransient(x => _exceptionHandlerMock.Object);

            _messageDispatcher = new MessageDispatcher(serviceDescriptors.BuildServiceProvider(), Mock.Of<ILogger<MessageDispatcher>>());
        }

        [Fact]
        public async Task DispatchMessageTest()
        {
            // Arrange
            var message = new Message(null, new FrameMetaData(MessageType.MessageSendRequest, 0, 0), null, Array.Empty<byte>());
            _messageHandlerMock.SetupGet(mock => mock.MessageType).Returns(message.FrameMetaData.Type);
            _messageHandlerMock.Setup(
                mock => mock.HandleAsync(message, It.IsAny<ConcurrentDictionary<string, ClientInfo>>(), It.IsAny<CancellationToken>())
                ).Returns(Task.CompletedTask);

            // Act
            await _messageDispatcher.DispatchAsync(message, new ConcurrentDictionary<string, ClientInfo>());

            // Assert
            _messageHandlerMock.Verify(
                mock => mock.HandleAsync(message, It.IsAny<ConcurrentDictionary<string, ClientInfo>>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task DispatchMessageWithHandlerExceptionMock()
        {
            // Arrange
            var socket = new Socket(SocketType.Stream, ProtocolType.Unspecified);
            var clientInfo = ClientInfo.Create(1, "test", socket);
            var frameMeta = new FrameMetaData(MessageType.MessageSendRequest, 0, 0);
            var message = new Message(clientInfo, frameMeta, null, Array.Empty<byte>());
            _messageHandlerMock.SetupGet(mock => mock.MessageType).Returns(frameMeta.Type);
            _messageHandlerMock.Setup(
                mock => mock.HandleAsync(message, It.IsAny<ConcurrentDictionary<string, ClientInfo>>(), It.IsAny<CancellationToken>()))
                    .Throws<MockedException>();

            // Act
            await _messageDispatcher.DispatchAsync(message, new ConcurrentDictionary<string, ClientInfo>());

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
            var clientInfo = ClientInfo.Create(1, "test", socket);

            // Act
            await _messageDispatcher.OnExceptionAsync(clientInfo, new MockedException());

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
