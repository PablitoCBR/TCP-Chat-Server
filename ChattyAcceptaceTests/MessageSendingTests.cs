using Core.Models.Consts;
using Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChattyAcceptaceTests
{
    public class MessageSendingTests : RunningServerTestTemplate
    {
        [Fact]
        public void SendingMessageTest()
        {
            Socket senderSocket = getAuthenticatedUserSocket("sender", "sender");
            Socket recipientSocket = getAuthenticatedUserSocket("recipient", "recipient");

            Task.Run(() => TryReciveMessage(recipientSocket, out byte[] message));
            bool sendingResult = TrySendMessage(senderSocket, "sender", "recipient");
            Assert.True(sendingResult);
        }

        [Fact]
        public void SendingMessageToNotConnectedUserTest()
        {
            Socket senderSocket = getAuthenticatedUserSocket("admin", "admin");
            bool result = TrySendMessage(senderSocket, "admin", "fake_name");
            Assert.False(result);
        }

        [Fact]
        public async Task ReciveingMessageTest()
        {
            Socket senderSocket = getAuthenticatedUserSocket("senderX", "sender");
            Socket recipientSocket = getAuthenticatedUserSocket("recipientX", "recipient");

            Task<bool> reciveResult = Task.Run(() => TryReciveMessage(recipientSocket, out byte[] message));
            bool sendingResult = TrySendMessage(senderSocket, "senderX", "recipientX");
            Assert.True(sendingResult);
            Assert.True(await reciveResult);
        }

        private Socket getAuthenticatedUserSocket(string username, string password)
        {
            Socket socket = CreateClientSocketConnectedToServer();
            TryRegisterUser(socket, username, password);
            socket.Dispose();
            socket = CreateClientSocketConnectedToServer();
            TryAuthenticateUser(socket, username, password);
            return socket;
        }

        private bool TrySendMessage(Socket senderSocket, string senderName, string recipient)
        {
            var headers = new Dictionary<string, string>
            {
                { MessageHeaders.Sender,  senderName},
                { MessageHeaders.Recipient, recipient }
            };

            byte[] message = BuildMessageBytes(MessageType.MessageSendRequest, headers);
            senderSocket.Send(message);
            byte[] response = ReciveMessage(senderSocket);

            return ((MessageType)response[0]) == MessageType.MessageSent;
        }

        private bool TryReciveMessage(Socket recipientSocket, out byte[] message)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                message = ReciveMessage(recipientSocket, cts.Token);
                return true;

            }
            catch(TaskCanceledException)
            {
                message = Array.Empty<byte>();
                return false;
            }
        }
    }
}
