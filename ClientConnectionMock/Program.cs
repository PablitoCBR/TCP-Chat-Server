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
        static void Main(string[] args)
        {
            try
            {

                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 8000);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                Socket sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    sender.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sender.RemoteEndPoint.ToString());

                    // Creation of messagge that 
                    // we will send to Server 
                    byte[] type = new byte[] { 0x35 };
                    byte[] headers = Encoding.ASCII.GetBytes("Authentication:" + Convert.ToBase64String(Encoding.ASCII.GetBytes("pablito:password")));
                    byte[] id = BitConverter.GetBytes(0);
                    byte[] messageLength = BitConverter.GetBytes(0);
                    byte[] headersLEngth = BitConverter.GetBytes(headers.Length);
                    List<byte> message = new List<byte>();
                    message.AddRange(type);
                    message.AddRange(id);
                    message.AddRange(headersLEngth);
                    message.AddRange(messageLength);
                    message.AddRange(headers);

                    int byteSent = sender.Send(message.ToArray());

                    // Data buffer 
                    byte[] messageReceived = new byte[1024];

                    // We receive the messagge using  
                    // the method Receive(). This  
                    // method returns number of bytes 
                    // received, that we'll use to  
                    // convert them to string 



                    int byteRecv = sender.Receive(messageReceived);
                    Console.WriteLine("Message from Server -> {0}",
                          Encoding.ASCII.GetString(messageReceived,
                                                     0, byteRecv));

                    Console.WriteLine("Press any key to log in");
                    Console.ReadKey();

                    sender.Close();
                    sender  = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(localEndPoint);

                    type = new byte[] { 0x36 };
                    headers = Encoding.ASCII.GetBytes("Authentication:" + Convert.ToBase64String(Encoding.ASCII.GetBytes("pablito:password")));
                    id = BitConverter.GetBytes(0);
                    messageLength = BitConverter.GetBytes(0);
                    headersLEngth = BitConverter.GetBytes(headers.Length);
                    message = new List<byte>();
                    message.AddRange(type);
                    message.AddRange(id);
                    message.AddRange(headersLEngth);
                    message.AddRange(messageLength);
                    message.AddRange(headers);

                    byteSent = sender.Send(message.ToArray());

                    // Data buffer 
                    messageReceived = new byte[1024];

                    // We receive the messagge using  
                    // the method Receive(). This  
                    // method returns number of bytes 
                    // received, that we'll use to  
                    // convert them to string 



                    byteRecv = sender.Receive(messageReceived);
                    Console.WriteLine("Message from Server -> {0}",
                          Encoding.ASCII.GetString(messageReceived,
                                                     0, byteRecv));

                    Console.WriteLine("Press any key to close");
                    Console.ReadKey();
                    sender.Close();
                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }
    }
}