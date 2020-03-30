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
using Utils;
using Xunit;

namespace ChattyLoadTests
{
    public class MultipleActionsAtOnce : RunningServerTestTemplate
    {
        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        public async Task MultipleConnectionsAtOnce(int amount)
        {
            List<Task<Socket>> connectionTasks = Enumerable.Range(0, amount).Select(x => new Task<Socket>(CreateClientSocketConnectedToServer)).ToList();
            connectionTasks.ForEach(task => task.Start());
            var result = await Task.WhenAll(connectionTasks.AsEnumerable());
            CollectionAssert.AssertTrueForAll(result, x => x.Connected);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        public async Task MultipleRegistrationAtOnce(int amount)
        {
            List<Task<Socket>> connectionTasks = Enumerable.Range(0, amount).Select(x => new Task<Socket>(CreateClientSocketConnectedToServer)).ToList();
            connectionTasks.ForEach(task => task.Start());
            Socket[] result = await Task.WhenAll(connectionTasks.ToArray());
            Assert.True(result.All(socket => socket.Connected));

            List<Task<bool>> registrationTasks = result.Select(socket => new Task<bool>(() => TryRegisterUser(socket, Guid.NewGuid().ToString(), "password"))).ToList();
            registrationTasks.ForEach(task => task.Start());
            var results = await Task.WhenAll(registrationTasks.ToArray());
            CollectionAssert.AssertTrueForAll(results, x => x);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        public async Task MultipleRegistrationAndAuthenticationAtOnce(int amount)
        {
            var tasks = Enumerable.Range(0, amount).Select(x => new Task(() =>
            {
                Socket socket = CreateClientSocketConnectedToServer();
                Guid username = Guid.NewGuid();
                bool registerResult = TryRegisterUser(socket, username.ToString(), "password");
                Assert.True(registerResult);
                socket = CreateClientSocketConnectedToServer();
                bool authenticationResult = TryAuthenticateUser(socket, username.ToString(), "password");
                Assert.True(authenticationResult);
            })).ToList();

            tasks.ForEach(task => task.Start());
            await Task.WhenAll(tasks.ToArray());
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(200)]
        public async Task MulitpleMessageSendingAtOnce(int amount)
        {
            var sendingTasksNotStarted = Enumerable.Range(0, amount).Select(x =>
            {
                Socket socket = CreateClientSocketConnectedToServer();
                Guid sender = Guid.NewGuid();
                Guid recipient = Guid.NewGuid();
                bool registerResult = TryRegisterUser(socket, sender.ToString(), "password");
                Assert.True(registerResult);
                socket = CreateClientSocketConnectedToServer();
                registerResult = TryRegisterUser(socket, recipient.ToString(), "password");
                Assert.True(registerResult);

                Socket senderSocket = CreateClientSocketConnectedToServer();
                Socket recipientSocket = CreateClientSocketConnectedToServer();

                bool authenticationResult = TryAuthenticateUser(senderSocket, sender.ToString(), "password");
                Assert.True(authenticationResult);
                authenticationResult = TryAuthenticateUser(recipientSocket, recipient.ToString(), "password");
                Assert.True(authenticationResult);

                return new Task(() =>
               {
                   Task<bool> reciveTask = Task.Run(() => TryReciveMessage(recipientSocket, out byte[] message));
                   bool sendingResult = TrySendMessage(senderSocket, sender.ToString(), recipient.ToString());
                   Assert.True(sendingResult);
                   Assert.True(reciveTask.Result);
               });
            }).ToList();

            sendingTasksNotStarted.ForEach(task => task.Start());
            await Task.WhenAll(sendingTasksNotStarted);
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
