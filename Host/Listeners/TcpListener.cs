using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;

using Host.Listeners.Interfaces;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Concurrent;
using Core.Models.Interfaces;
using Core.Services.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Microsoft.Extensions.Options;
using Core.MessageHandlers.Interfaces;

namespace Host.Listeners
{
    public class TcpListener : IListener
    {
        public ProtocolType ProtocolType { get; }

        public bool IsListening { get; private set; }

        private ILogger<IListener> Logger { get; }

        private ListennerSettings Settings { get; set; }

        private IPEndPoint IPEndPoint { get; }

        private ManualResetEvent Received { get; }

        private CancellationToken CancellationToken { get; set; }

        private ConcurrentDictionary<string, IClientInfo> ConnectedClients { get; }

        private IFrameMetaEncoder FrameMetaEncoder { get; }

        private FrameMetaDataConfiguration FrameMetaDataConfiguration { get; }

        private IMessageEncoder MessageEncoder { get; }

        private IAuthenticationHandler AuthenticationHandler { get; }

        public TcpListener(ListennerSettings settings, IPEndPoint ipEndPoint, ILogger<IListener> logger, IFrameMetaEncoder frameMetaEncoder, 
            IOptions<FrameMetaDataConfiguration> frameMetaDataConfiguration, IMessageEncoder messageEncoder, IAuthenticationHandler authenticationHandler)
        {
            Logger = logger;
            ProtocolType = ProtocolType.Tcp;
            IsListening = false;
            Settings = settings;
            IPEndPoint = ipEndPoint;
            Received = new ManualResetEvent(false);
            ConnectedClients = new ConcurrentDictionary<string, IClientInfo>();
            FrameMetaEncoder = frameMetaEncoder;
            FrameMetaDataConfiguration = frameMetaDataConfiguration.Value;
            MessageEncoder = messageEncoder;
            AuthenticationHandler = authenticationHandler;

            Logger.LogInformation("TCP Listener created and assigned to port: {0}", IPEndPoint.Port);
        }

        public void Listen(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            Logger.LogInformation("Starting listening for TCP transimssions on port: {0}, IP: {1}", IPEndPoint.Port, IPEndPoint.Address.ToString());

            Socket listener = new Socket(IPEndPoint.AddressFamily, SocketType.Stream, ProtocolType);
            listener.Bind(IPEndPoint);
            listener.Listen(Settings.PendingConnectionsQueue);
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
                byte[] frameMetaData = await ReceiveDataAsync(connectedClientSocket, this.FrameMetaDataConfiguration.MetaDataLength);
                IFrameMetaData metaData = this.FrameMetaEncoder.Decode(frameMetaData);

                if (!(metaData.Type == MessageType.Registration || metaData.Type == MessageType.Authentication))
                {
                    // Invalid connection message, discard connection    
                }
                else
                {
                    byte[] data = await this.ReceiveDataAsync(connectedClientSocket, metaData.HeadersDataLength + metaData.MessageDataLength);
                    IMessage message = this.MessageEncoder.Decode(data, metaData, ClientInfo.Create(0, string.Empty, connectedClientSocket));

                    if (metaData.Type == MessageType.Registration)
                        await this.AuthenticationHandler.RegisterAsync(message);
                    else
                    {
                        IClientInfo client = await this.AuthenticationHandler.Authenticate(message);
                        this.RegisterConnectedUser(client);
                        this.ListenForMessagesAsync(client);
                    }
                }

            }
            catch (Exception ex)
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

        private void RegisterConnectedUser(IClientInfo clientInfo)
        {
            bool result = this.ConnectedClients.TryAdd(clientInfo.Name, clientInfo);

            if (result) return;

            this.Logger.LogError("Failed to add user with ID: {0} to ConnectedClients collection", clientInfo.Id);
        }

        private async void ListenForMessagesAsync(IClientInfo clientInfo)
        {
            while (!this.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] frameMetaData = await this.ReceiveDataAsync(clientInfo.Socket, this.FrameMetaDataConfiguration.MetaDataLength);
                    IFrameMetaData frameMeta = this.FrameMetaEncoder.Decode(frameMetaData);
                    byte[] data = await this.ReceiveDataAsync(clientInfo.Socket, frameMeta.HeadersDataLength + frameMeta.MessageDataLength);
                    IMessage message = this.MessageEncoder.Decode(data, frameMeta, clientInfo);
                    //await this.Dispatcher.DispatcheAsync(message, this.ConnectedClients);
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException is SocketException)
                        this.Logger.LogInformation("Connection with: {0} was closed by user.", clientInfo.Name);
                    else
                    {
                        this.Logger.LogError(ex, "Exception occured while listening for messages from {0} ({1}).",
                           clientInfo.Name,
                            clientInfo.RemoteEndPoint.ToString());
                    }

                    break;
                }
            }

            clientInfo.Socket.Close();
            this.ConnectedClients.TryRemove(clientInfo.Name, out IClientInfo result);
        }


        private async Task<byte[]> ReceiveDataAsync(Socket clientSocket, int dataLength)
        {
            if (CancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<byte[]>(CancellationToken);


            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[dataLength]);
            try
            {
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None, this.CancellationToken);
            }
            catch (AggregateException ex)
            {
                Logger.LogError(ex, "Data receiving from {0} has to stopped due to exception: {1}",
                        ((IPEndPoint)clientSocket?.RemoteEndPoint)?.Address.ToString(), ex.Message);
                return await Task.FromException<byte[]>(ex);
            }

            return buffer.Array;
        }
    }
}
