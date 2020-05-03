using Core.Models;
using Core.Models.Consts;
using Core.Models.Enums;
using Core.Services.Encoders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class ConsoleClient
    {
        public bool LoggedIn { get; private set; } = false;
        public string Username { get; }

        private const int HOST_PORT = 8000;
        private readonly string _password;

        private byte[] _lastRecivedMessage;
        private ManualResetEvent _messageRecived;
        private ManualResetEvent _messageHandled;
        private EventHandler<byte[]> _messageRecivedEvent;
        private Socket _socket;

        private readonly IFrameMetaEncoder _frameMetaEncoder;
        private readonly IHeadersEncoder _headersEncoder;

        public ConsoleClient(string username, string password)
        {
            Username = username;
            _password = password;
            _frameMetaEncoder = new FrameMetaEncoder(
                Options.Create(
                    new FrameMetaDataConfiguration() { HeadersLengthFieldSize = 4, MessageLengthFieldSize = 4, MetaDataFieldsTotalSize = 9 }));
            _headersEncoder = new HeadersEncoder();
        }

        public void Register()
        {
            ensureConnected();
            var headers = new Dictionary<string, string> { { MessageHeaders.Authentication, $"{Username}:{_password}" } };
            byte[] message = buildMessageBytes(MessageType.RegistrationRequest, headers);

            logInfo("Sending registration message.");
            _socket.Send(message);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                byte[] response = reciveMessage(_socket, cts.Token);

                if (((MessageType)response[0]) == MessageType.Registered)
                    logInfo($"User: {Username} registered successfully.");
                else logError("Registration failed!");
            }
        }

        public void Login()
        {
            ensureConnected();
            var headers = new Dictionary<string, string> { { MessageHeaders.Authentication, $"{Username}:{_password}" } };
            byte[] message = buildMessageBytes(MessageType.AuthenticationRequest, headers);

            logInfo("Sending authentication message.");
            _socket.Send(message);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                byte[] response = reciveMessage(_socket, cts.Token);

                if (((MessageType)response[0]) == MessageType.Authenticated)
                {
                    logInfo($"User: {Username} authenticated successfully.");
                    LoggedIn = true;
                    Task.Run(() => listenForMessages());
                    _messageHandled = new ManualResetEvent(false);
                    _messageRecived = new ManualResetEvent(false);
                    _messageRecivedEvent += handleRecivedMessage;
                    Thread.Sleep(1000);
                }

                else logError("Authentication failed!");
            }
        }

        public void SendMessage(string recipient, string message)
        {
            _messageRecivedEvent -= handleRecivedMessage;

            var headers = new Dictionary<string, string>
            {
                { MessageHeaders.Sender,  Username},
                { MessageHeaders.Recipient, recipient }
            };

            byte[] messageData = buildMessageBytes(MessageType.MessageSendRequest, headers, message);
            _socket.Send(messageData);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                _messageRecived.WaitOne();
                if (((MessageType)_lastRecivedMessage[0]) == MessageType.MessageSent)
                    logInfo($"Message sent successfully.");
                else if (((MessageType)_lastRecivedMessage[0]) == MessageType.ClientUnreachable)
                    logError("Recipient not connected!");
                else logError($"Error occured: {(MessageType)_lastRecivedMessage[0]}");
                _messageRecived.Reset();
                _messageHandled.Set();
            }

            _messageRecivedEvent += handleRecivedMessage;
        }

        private void listenForMessages()
        {
            while(true)
            {
                byte[] message = reciveMessage(_socket);
                _lastRecivedMessage = message;
                _messageRecived.Set();
                _messageRecivedEvent?.Invoke(this, message);
                _messageHandled.WaitOne();
            }
        }

        private void handleRecivedMessage(object sender, byte[] message)
        {
            if(((MessageType)_lastRecivedMessage[0]) != MessageType.Message)
            {
                logError($"Recived: {(MessageType)_lastRecivedMessage[0]}.");
            }
            else
            {
                FrameMetaData frameMetaData = _frameMetaEncoder.Decode(_lastRecivedMessage.Take(9).ToArray());
                IDictionary<string, string> headers = _headersEncoder.Decode(_lastRecivedMessage.Skip(9).Take(frameMetaData.HeadersDataLength).ToArray());
                string messageBody = Encoding.ASCII.GetString(message.Skip(9 + frameMetaData.HeadersDataLength).ToArray());
                logMessageRecived(headers[MessageHeaders.Sender], messageBody);
            }

            _messageRecived.Reset();
            _messageHandled.Set();
        }

        private void ensureConnected()
        {
            if (_socket != null && _socket.Connected) return;
#if DEBUG
            IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[0];
#else
            Console.Write("Enter server host IPv4 address: ");
            string ipv4 = Console.ReadLine().Trim();
            IPAddress ipAddress = IPAddress.Parse(ipv4);
#endif
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEndpoint = new IPEndPoint(ipAddress, HOST_PORT);
            logInfo("Connecting to host.");
            try
            {
                _socket.Connect(serverEndpoint);
                logInfo("Connection established.");
            }
            catch (Exception)
            {
                logError("Failed to connect!");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private void logInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[INFO] {0}", message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void logError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] {0}", message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void logMessageRecived(string recipient, string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[MSG FROM: {0}] {1}", recipient, message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private byte[] buildMessageBytes(MessageType messageType, IDictionary<string, string> headers, string message = null)
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
            byte[] messageData = Encoding.ASCII.GetBytes(message ?? string.Empty);
            byte[] headersBytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            var data = new List<byte> { (byte)messageType };
            data.AddRange(BitConverter.GetBytes(headersBytes.Length));
            data.AddRange(BitConverter.GetBytes(messageData.Length));
            data.AddRange(headersBytes);
            data.AddRange(messageData);

            return data.ToArray();
        }

        private byte[] reciveMessage(Socket socket, CancellationToken cancellationToken = default)
        {
            byte[] metaBuffer = new byte[9];
            socket.Receive(metaBuffer, SocketFlags.None);

            int headerLength = BitConverter.ToInt32(metaBuffer.Skip(1).Take(4).ToArray());
            int messageLength = BitConverter.ToInt32(metaBuffer.Skip(5).Take(4).ToArray());

            if (headerLength + messageLength == 0)
                return metaBuffer;

            byte[] dataBuffer = new byte[headerLength + messageLength];
            socket.Receive(dataBuffer, SocketFlags.None);

            var data = new List<byte>();
            data.AddRange(metaBuffer);
            data.AddRange(dataBuffer);
            return data.ToArray();
        }
    }
}
