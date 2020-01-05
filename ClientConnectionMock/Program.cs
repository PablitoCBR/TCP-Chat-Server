using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        const byte RegistrationRequest = 0x36;
        const byte AuthenticationRequest = 0x37;
        const byte MessageSendRequest = 0x33;
        const byte Registered = 0x02;
        const byte Authenticated = 0x03;
        const byte MessageSent = 0x04;

        const int Port = 8000;

        static string Username;
        static string Password;

        static Socket socket;
        static IPAddress ipAddress;

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHost.AddressList[0];

            try
            {
                CollectCredentials();

                Console.Clear();

                // Login / register
                string action = string.Empty;
                do
                {
                    Console.WriteLine("What you want to do?");
                    Console.WriteLine("1. Regiser");
                    Console.WriteLine("2. Login");
                    Console.Write("Choice: ");
                    action = Console.ReadLine();
                    Console.Clear();

                    switch (action)
                    {
                        case "1":
                            Register();
                            break;
                        case "2":
                            Login();
                            break;
                        default:
                            break;
                    }
                }
                while (!string.Equals(action,"2"));


                while(true)
                {
                    Console.Write("Recipient: ");
                    string recipient = Console.ReadLine();
                    Console.Write("Message: ");
                    string message = Console.ReadLine();

                    SendMessage(recipient, message, socket);
                    Console.WriteLine();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exceptino occured. Message: {ex.Message}");
            }
        }

        static void SendMessage(string recipient, string message, Socket socket)
        {
            string headerString = $"recipient:{recipient}\n";
            byte[] headers = Encoding.ASCII.GetBytes(headerString);
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            List<byte> data = new List<byte> { MessageSendRequest };
            data.AddRange(BitConverter.GetBytes(headers.Length));
            data.AddRange(BitConverter.GetBytes(messageBytes.Length));
            data.AddRange(headers);
            data.AddRange(messageBytes);
            byte[] messageData = data.ToArray();

            Console.WriteLine($"Sending message to {recipient}.");
            socket.Send(messageData);
            byte[] response = ReciveMessage();

            if(response[0] == MessageSent)
                Console.WriteLine($"Message sent to {recipient}");
            else
            {
                Console.WriteLine("Failed to send message...");
            }
        }

        static void Register()
        {
            // Try to connect to server.
            Console.WriteLine("Server connection attempt.");
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
            socket.Connect(localEndPoint);
            Console.WriteLine("Listener connected to server.");

            socket.Send(GetRegistrationMessageBytes());
            byte[] mesasge = ReciveMessage();

            if(mesasge[0] != Registered)
            {
                Console.WriteLine("Registration failed!");
                throw new Exception("Registratino failed");
            }
        }

        static void Login()
        {
            // Try to connect to server.
            Console.WriteLine("Server connection attempt.");
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
            socket.Connect(localEndPoint);
            Console.WriteLine("Listener connected to server.");

            socket.Send(GetAuthentiactionMessageBytes());
            byte[] message = ReciveMessage();

            if(message[0] != Authenticated)
            {
                Console.WriteLine("Authentication failed!");
                throw new Exception("Authentication failed.");
            }
        }

        static void CollectCredentials()
        {
            Console.Write("Username: ");
            Username = Console.ReadLine();
            Console.Write("Password: ");
            Password = Console.ReadLine();
            Console.Clear();
        }

        static byte[] GetAuthentiactionMessageBytes()
        {
            byte[] headers = Encoding.ASCII.GetBytes("authentication:" + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}")));
            byte[] messageLength = BitConverter.GetBytes(0);
            byte[] headersLength = BitConverter.GetBytes(headers.Length);
            List<byte> data = new List<byte> { AuthenticationRequest };
            data.AddRange(headersLength);
            data.AddRange(messageLength);
            data.AddRange(headers);
            return data.ToArray();
        }

        static byte[] GetRegistrationMessageBytes()
        {
            byte[] headers = Encoding.ASCII.GetBytes("authentication:" + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}")));
            byte[] messageLength = BitConverter.GetBytes(0);
            byte[] headersLength = BitConverter.GetBytes(headers.Length);
            List<byte> data = new List<byte> { RegistrationRequest };
            data.AddRange(headersLength);
            data.AddRange(messageLength);
            data.AddRange(headers);
            return data.ToArray();
        }

        static byte[] ReciveMessage()
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