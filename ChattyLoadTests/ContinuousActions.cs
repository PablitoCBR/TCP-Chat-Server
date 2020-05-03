using ChattyAcceptaceTests;
using Core.Models.Consts;
using Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChattyLoadTests
{
    public class ContinuousActions : RunningServerTestTemplate
    {
        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(100)]
        [InlineData(1000)]
        public void MultipleConsecutiveConnections(int amount)
        {
            var connectedSockets = new List<Socket>();

            for (int i = 0; i < amount; i++)
            {
                Socket socket = CreateClientSocketConnectedToServer();
                Assert.True(socket.Connected);
                connectedSockets.Add(socket);
            }

            Assert.True(connectedSockets.TrueForAll(socket => socket.Connected));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(100)]
        [InlineData(500)]
        public void MultipleConsecutiveRegistrations(int amount)
        {
            var connectedSockets = Enumerable.Range(0, amount).Select(x => CreateClientSocketConnectedToServer());

            var watch = new Stopwatch();
            watch.Start();
            foreach(Socket socket in connectedSockets)
            {
                bool registrationResult = TryRegisterUser(socket, Guid.NewGuid().ToString(), "password");
                Assert.True(registrationResult);
            }
            watch.Stop();
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(100)]
        [InlineData(500)]
        public void MultipleConsecutiveRegistrationsAndAuthentication(int amount)
        {
            var connectedSockets = Enumerable.Range(0, amount).Select(x => CreateClientSocketConnectedToServer()).ToArray();

            var watch = new Stopwatch();
            watch.Start();
            for(int index = 0; index < amount; index++)
            {
                Guid userGuid = Guid.NewGuid();
                bool registrationResult = TryRegisterUser(connectedSockets[index], userGuid.ToString(), "password");
                connectedSockets[index].Dispose();
                connectedSockets[index] = CreateClientSocketConnectedToServer();
                Assert.True(registrationResult);
                bool authenticationResult = TryAuthenticateUser(connectedSockets[index], userGuid.ToString(), "password");
                Assert.True(authenticationResult);
            }
            watch.Stop();
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(200)]
        public void MultipleConsecutiveMessageSending(int amount)
        {
            var connectedSockets = Enumerable.Range(0, amount).Select(x => CreateClientSocketConnectedToServer());
            var users = new Dictionary<Guid, Socket>();

            foreach (Socket socket in connectedSockets)
            {
                Guid userGuid = Guid.NewGuid();
                bool registrationResult = TryRegisterUser(socket, userGuid.ToString(), "password");
                Assert.True(registrationResult);
                Socket authenticatedSocket = CreateClientSocketConnectedToServer();
                bool authenticationResult = TryAuthenticateUser(authenticatedSocket, userGuid.ToString(), "password");
                users.Add(userGuid, authenticatedSocket);
            }

            var watch = new Stopwatch();
            watch.Start();
            foreach(var (guid, socket) in users)
            {
                var recipient = users.FirstOrDefault(x => x.Key != guid);
                Task<bool> reciveTask = Task.Run(() => TryReciveMessage(recipient.Value, out byte[] message));
                bool sendingResult = TrySendMessage(socket, guid.ToString(), recipient.Key.ToString());
                Assert.True(sendingResult);
                Assert.True(reciveTask.Result);
            }
            watch.Stop();
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
            catch (TaskCanceledException)
            {
                message = Array.Empty<byte>();
                return false;
            }
        }
    }
}
