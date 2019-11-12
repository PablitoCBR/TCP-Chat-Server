using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;

using Host.Listeners.Interfaces;
using System.Threading.Tasks;
using Core.Models.Interfaces;
using Core.Models;
using System.Text;

namespace Host.Listeners
{
    public class TcpListener : IListener
    {
        public ProtocolType ProtocolType { get; }

        public bool IsListening { get; private set; }

        private ILogger<IListener> Logger { get; }

        private int ConnectionsQueueLength { get; }

        private IPEndPoint IPEndPoint { get; }

        private ManualResetEvent Received { get; }

        private CancellationToken CancellationToken { get; set; }
        

        public TcpListener(ListennerSettings settings, IPEndPoint ipEndPoint, ILogger<IListener> logger)
        {
            Logger = logger;
            ProtocolType = ProtocolType.Tcp;
            IsListening = false;
            ConnectionsQueueLength = settings.PendingConnectionsQueue;
            IPEndPoint = ipEndPoint;
            Received = new ManualResetEvent(false);
            Logger.LogInformation("TCP Listener created and assigned to port: {0}", IPEndPoint.Port);
        }

        public void Listen(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            Logger.LogInformation("Starting listening for TCP transimssions on port: {0}, IP: {1}", IPEndPoint.Port, IPEndPoint.Address.ToString());

            Socket listener = new Socket(IPEndPoint.AddressFamily, SocketType.Stream, ProtocolType);
            listener.Bind(IPEndPoint);
            listener.Listen(ConnectionsQueueLength);
            IsListening = true;

            while (!CancellationToken.IsCancellationRequested)
            {
                Received.Reset();
                listener.BeginAccept(new AsyncCallback(ConnectionHandleCallback), listener);
                Received.WaitOne();
            }

            try
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
            catch (SocketException)
            {
                Logger.LogInformation("TCP Listener was closed.");
            }
            finally
            {
                IsListening = false;
            }
        }

        private async void ConnectionHandleCallback(IAsyncResult asyncResult)
        {
            Socket socket = asyncResult.AsyncState as Socket;

            if (CancellationToken.IsCancellationRequested)
                return;

            Received.Set(); // Signal main thread to continnue listening

            Socket connectedClientSocket = socket.EndAccept(asyncResult); // Accept pending connection and create socket
            Logger.LogInformation("Connection accepted from: {0}", ((IPEndPoint)connectedClientSocket.RemoteEndPoint).Address.ToString());
            try
            {
                byte[] frameMetaData = await ReceiveMessageFrameMetaData(connectedClientSocket);
                IFrameMetaData frameMeta = FrameMetaData.Create(frameMetaData);
            }
            catch(Exception ex)
            {
                if (CancellationToken.IsCancellationRequested)
                    Logger.LogWarning("Listening caceled while receiving frame from: {0}", ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());
                else
                {
                    Logger.LogError(ex, "Exception occured while receiving and creating frame metadata from: {0}. Check Logs for more info!",
                        ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());
                }
                
                connectedClientSocket.Close();
                return;
            }

            connectedClientSocket.Send(Encoding.ASCII.GetBytes("Message received! Its just test signal response!"));
        }

        private async Task<byte[]> ReceiveMessageFrameMetaData(Socket clientSocket)
        {
            if (CancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<byte[]>(CancellationToken);


            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[13]);
            try
            {
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
            }
            catch (AggregateException ex)
            {
                Logger.LogError("Data receiving from {0} has to stopped due to exception: {1}",
                        ((IPEndPoint)clientSocket?.RemoteEndPoint)?.Address.ToString(), ex.Message);
                return await Task.FromException<byte[]>(ex);
            }
            
            return buffer.Array;
        }
    }
}
