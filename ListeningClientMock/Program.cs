using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ListeningClientMock
{
    class Program
    {
        const string Username = "MockedUser";
        const string Password = "MockedUserPassword";

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];

            Socket listeningSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                // Try to connect to server.
                Console.WriteLine("Server connection attempt.");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8000);
                listeningSocket.Connect(localEndPoint);
                Console.WriteLine("Listener connected to server.");

                // Loggin into server.
                Console.WriteLine("Listener attempt to log into server.");
                byte[] authentiactionMessage = GetAuthentiactionMessageBytes();
                listeningSocket.Send(authentiactionMessage);

                byte[] authenticationResponse = new byte[9];
                listeningSocket.Receive(authenticationResponse, authenticationResponse.Length, SocketFlags.None);
                if(authenticationResponse[0] == 0x03)
                    Console.WriteLine("Client authenticated.");
                else
                {
                    Console.WriteLine("Authentication failed. Aborting.");
                    throw new Exception("Authentication Failed!");
                }


                // Listening and responding to messages
                Console.WriteLine("Listening for incoming messages.");
                while(true)
                {
                    byte[] metaBuffer = new byte[9];
                    listeningSocket.Receive(metaBuffer, SocketFlags.None);

                    if(metaBuffer[0] == 0x01)
                        Console.WriteLine("Recived message.");
                    else
                    {
                        Console.WriteLine($"Unkown message type. Type Code: {metaBuffer[0]}. Aborting!");
                        throw new Exception("Unkown message type.");
                    }

                    int headerLength = BitConverter.ToInt32(metaBuffer.Skip(1).Take(4).ToArray());
                    int messageLength = BitConverter.ToInt32(metaBuffer.Skip(5).Take(4).ToArray());

                    byte[] messageBuffer = new byte[headerLength + messageLength];
                    listeningSocket.Receive(messageBuffer, SocketFlags.None);

                    string message = Encoding.ASCII.GetString(messageBuffer.Skip(headerLength).Take(messageLength).ToArray());
                    Console.WriteLine($"Recived message: {message}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception occured. Message: {ex.Message}");
            }
        }

        static byte[] GetAuthentiactionMessageBytes()
        {
            byte messageType = 0x37;
            byte[] headers = Encoding.ASCII.GetBytes("authentication:" + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}")));
            byte[] messageLength = BitConverter.GetBytes(0);
            byte[] headersLength = BitConverter.GetBytes(headers.Length);
            List<byte> data = new List<byte> { messageType };
            data.AddRange(headersLength);
            data.AddRange(messageLength);
            data.AddRange(headers);
            return data.ToArray();
        }
    }
}
