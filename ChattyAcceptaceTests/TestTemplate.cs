using Core.Models.Consts;
using Core.Models.Enums;
using Core.Services.Encoders;
using Core.Services.Encoders.Interfaces;
using Host;
using Host.Builder;
using Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChattyAcceptaceTests
{
    public abstract class TestTemplate
    {
        protected IHost Host { get; }
        protected int HostPort { get; }

        protected TestTemplate()
        {
            IHostBuilder builder = Server.Server.CreateDeafaultBuilder<Startup>(Array.Empty<string>());
            HostPort = builder.BuilderSettings.Port;
            Host = builder.Build();
        }

        protected Socket CreateClientSocketConnectedToServer()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndpoint = new IPEndPoint(ipAddress, HostPort);

            Trace.WriteLine("Server connection attempt.");
            socket.Connect(serverEndpoint);
            Trace.WriteLine("Connection established.");

            return socket;
        }

        protected Task StartHostServerAsync()
        {
            Trace.WriteLine("Starting server host...");
            Host.Run();
            Trace.WriteLine("Server host started.");
            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        protected Task StopHostServerAsync()
        {
            Trace.WriteLine("Stoping server host...");
            Host.Stop();
            Trace.WriteLine("Server host stopped.");
            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        protected bool TryRegisterUser(Socket clientSocket, string username, string password)
        {
            var headers = new Dictionary<string, string> { { MessageHeaders.Authentication, $"{username}:{password}" } };
            byte[] message = BuildMessageBytes(MessageType.RegistrationRequest, headers);

            Trace.WriteLine("Sending registration request...");
            clientSocket.Send(message);
            Trace.WriteLine("Request send. Waiting for response...");

            byte[] response = ReciveMessage(clientSocket);
            Trace.WriteLine("Response recived.");

            if(((MessageType)response[0]) == MessageType.Registered)
            {
                Trace.WriteLine($"User: {username} registered successfully.");
                return true;
            }
            else
            {
                Trace.WriteLine("Failed to register user.");
                return false;
            }
        }

        protected bool TryAuthenticateUser(Socket clientSocket, string username, string password)
        {
            Trace.WriteLine($"Attempt to authenticate user: {username}");
            var headers = new Dictionary<string, string> { { MessageHeaders.Authentication, $"{username}:{password}"} };
            byte[] message = BuildMessageBytes(MessageType.AuthenticationRequest, headers);
            clientSocket.Send(message);
            byte[] response = ReciveMessage(clientSocket);

            if(((MessageType)response[0]) == MessageType.Authenticated)
            {
                Trace.WriteLine("User authenticated!");
                return true;
            }
            else
            {
                Trace.WriteLine("Faild to authenticate user.");
                return false;
            }
        }

        protected byte[] BuildMessageBytes(MessageType messageType, IDictionary<string, string> headers)
        {
            var stringBuilder = new StringBuilder();

            foreach (var keyValuePair in headers)
            {
                if (string.Equals(keyValuePair.Key, MessageHeaders.Authentication))
                {
                    stringBuilder.AppendLine($"{keyValuePair.Key}:{Convert.ToBase64String(Encoding.ASCII.GetBytes(keyValuePair.Value))}");
                }
                else
                {
                    stringBuilder.Append($"{keyValuePair.Key}:{keyValuePair.Value}\n");
                }
            }

            byte[] headersBytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            var data = new List<byte> { (byte)messageType };
            data.AddRange(BitConverter.GetBytes(headersBytes.Length));
            data.AddRange(BitConverter.GetBytes(0));
            data.AddRange(headersBytes);

            return data.ToArray();
        }

        protected byte[] ReciveMessage(Socket socket, CancellationToken cancellationToken = default)
        {
            byte[] metaBuffer = new byte[9];
            socket.Receive(metaBuffer, SocketFlags.None);

            int headerLength = BitConverter.ToInt32(metaBuffer.Skip(1).Take(4).ToArray());
            int messageLength = BitConverter.ToInt32(metaBuffer.Skip(5).Take(4).ToArray());

            if (headerLength + messageLength == 0)
                return metaBuffer;

            byte[] dataBuffer = new byte[headerLength + messageLength];
            var amount = socket.ReceiveAsync(dataBuffer, SocketFlags.None, cancellationToken).Result;

            var data = new List<byte>();
            data.AddRange(metaBuffer);
            data.AddRange(dataBuffer);
            return data.ToArray();
        }
    }
}
