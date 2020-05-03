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
        [InlineData(1000)]
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
            var connectionTasks = Enumerable.Range(0, amount).Select(x => Task.Run(() => CreateClientSocketConnectedToServer()));
            Socket[] result = await Task.WhenAll(connectionTasks.ToArray());
            Assert.True(result.All(socket => socket.Connected));

            var watch = new Stopwatch();
            watch.Start();
            var registrationTasks = result.Select(socket => Task.Run(() => TryRegisterUser(socket, Guid.NewGuid().ToString(), "password")));
            var results = await Task.WhenAll(registrationTasks.ToArray());
            CollectionAssert.AssertTrueForAll(results, x => x);
            watch.Stop();
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
            var connectedSockets = Enumerable.Range(0, amount).Select(x => CreateClientSocketConnectedToServer()).ToList();

            var watch = new Stopwatch();
            watch.Start();
            var tasks = connectedSockets.Select(socket => Task.Run(async () =>
            {
                Guid username = Guid.NewGuid();
                await Task.Run(() =>
                {
                    bool registerResult = TryRegisterUser(socket, username.ToString(), "password");
                    Assert.True(registerResult);
                    socket.Dispose();
                });
                await Task.Run(() =>
                {
                    socket = CreateClientSocketConnectedToServer();
                    bool authenticationResult = TryAuthenticateUser(socket, username.ToString(), "password");
                    Assert.True(authenticationResult);
                });
            }));

            await Task.WhenAll(tasks.ToArray());
            watch.Stop();
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

            var watch = new Stopwatch();
            watch.Start();
            sendingTasksNotStarted.ForEach(task => task.Start());
            await Task.WhenAll(sendingTasksNotStarted);

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
