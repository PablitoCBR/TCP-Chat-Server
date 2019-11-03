using Host.Listeners.Interfaces;
using Host.Models.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Host.Listeners
{
    public class TcpListener : IListener
    {
        public ProtocolType ProtocolType { get; }

        public bool IsListening { get; private set; }

        private ILogger<IListener> Logger { get; }
        
        private int ConnectionsQueueLength { get; }

        private IPEndPoint IPEndPoint { get; }

        private ConcurrentDictionary<int, IClientInfo> ActiveClients { get; }

        public TcpListener(int connectionQueueLimit, IPEndPoint ipEndPoint, ILogger<IListener> logger)
        {
            Logger = logger;
            ProtocolType = ProtocolType.Tcp;
            IsListening = false;
            ConnectionsQueueLength = connectionQueueLimit;
            IPEndPoint = ipEndPoint;

            ActiveClients = new ConcurrentDictionary<int, IClientInfo>();

            Logger.LogInformation("TCP Listener created and assigned to port: {0}", IPEndPoint.Port);
        }

        public void Listen(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting listening for TCP transimssions on port: {0}, IP: {1}", IPEndPoint.Port, IPEndPoint.Address.ToString());

            Socket listener = new Socket(IPEndPoint.AddressFamily, SocketType.Stream, ProtocolType);
            listener.Bind(IPEndPoint);
            listener.Listen(ConnectionsQueueLength);
            IsListening = true;
            
            while(!cancellationToken.IsCancellationRequested)
            {
                Socket client = listener.Accept();
                Task.Factory.StartNew(() => HandleConnectionAsync(client, cancellationToken), cancellationToken).ConfigureAwait(false);                
            }

            try
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
            catch(SocketException)
            {
                Logger.LogInformation("TCP Listener was closed. Attempting to close connections with clients...");

                foreach(var clientPair in ActiveClients)
                {
                    clientPair.Value.Socket.Shutdown(SocketShutdown.Both);
                    clientPair.Value.Socket.Close();
                }

                Logger.LogInformation("Client connections closed.");
            }
            finally
            {
                IsListening = false;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private async void HandleConnectionAsync(Socket tcpClient, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;


        }
    }
}
