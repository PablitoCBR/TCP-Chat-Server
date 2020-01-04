using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Host.Listeners.Interfaces;

using Core.Pipeline.Interfaces;

using Core.Models;
using Core.Models.Enums;
using Core.Models.Exceptions.UserFaultExceptions;
using Core.Models.Interfaces;

using Core.Services.Encoders.Interfaces;

using Core.Handlers.Security.Interfaces;

namespace Host.Listeners
{
    public class TcpListener : IListener
    {
        public ProtocolType ProtocolType { get; }
        public bool IsListening { get; private set; }

        private readonly ILogger<IListener> _logger;

        private readonly IServiceProvider _serviceProvider;

        private readonly ListennerSettings _listenerSettings;

        private readonly IPEndPoint _ipEndPoint;

        private readonly ConcurrentDictionary<string, IClientInfo> _connectedClients;

        private readonly FrameMetaDataConfiguration _frameMetaDataConfiguration;

        private readonly ManualResetEvent _acceptEvent;

        private CancellationToken _cancellationToken;

        public TcpListener(ListennerSettings settings, IPEndPoint ipEndPoint, ILogger<IListener> logger,
            IOptions<FrameMetaDataConfiguration> frameMetaDataConfiguration, IServiceProvider serviceProvider)
        {
            ProtocolType = ProtocolType.Tcp;
            IsListening = false;

            _connectedClients = new ConcurrentDictionary<string, IClientInfo>();

            _logger = logger;
            _listenerSettings = settings;
            _ipEndPoint = ipEndPoint;
            _frameMetaDataConfiguration = frameMetaDataConfiguration.Value;

            _serviceProvider = serviceProvider;
            _acceptEvent = new ManualResetEvent(false);

            _logger.LogInformation("TCP Listener created and assigned to port: {0}", _ipEndPoint.Port);
        }

        public void Listen(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting listening for TCP connections on port: {0}, IP: {1}", _ipEndPoint.Port, _ipEndPoint.Address.MapToIPv4().ToString());
            _cancellationToken = cancellationToken;

            Socket listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, this.ProtocolType);
            listener.Bind(_ipEndPoint);
            listener.Listen(_listenerSettings.PendingConnectionsQueue);

            IsListening = true;

            while (!_cancellationToken.IsCancellationRequested)
            {
                _acceptEvent.Reset();
                listener.BeginAccept(new AsyncCallback(HandleConnectionAttemptAsync), listener);
                _acceptEvent.WaitOne();
            }

            try
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
            catch (SocketException)
            {
                _logger.LogInformation("TCP Listener was closed.");
            }
            finally
            {
                IsListening = false;
            }
        }

        private async void HandleConnectionAttemptAsync(IAsyncResult asyncResult)
        {
            _acceptEvent.Set();

            if (_cancellationToken.IsCancellationRequested)
                return;

            Socket connectedClientSocket = ((Socket)asyncResult).EndAccept(asyncResult);
            _logger.LogInformation("Connection accepted from: {0}", ((IPEndPoint)connectedClientSocket.RemoteEndPoint).Address.ToString());

            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    byte[] frameMetaData = await ReceiveDataAsync(connectedClientSocket, _frameMetaDataConfiguration.MetaDataFieldsTotalSize);
                    IFrameMetaData metaData = scope.ServiceProvider.GetRequiredService<IFrameMetaEncoder>().Decode(frameMetaData);

                    if (!(metaData.Type == MessageType.RegistrationRequest || metaData.Type == MessageType.AuthenticationRequest))
                        throw new InvalidMessageException(metaData.Type, "Only registration and authentication requests allowed.");


                    byte[] data = await this.ReceiveDataAsync(connectedClientSocket, metaData.HeadersDataLength + metaData.MessageDataLength).ConfigureAwait(false);
                    IMessage message = scope.ServiceProvider.GetRequiredService<IMessageEncoder>().Decode(data, metaData, ClientInfo.Create(0, string.Empty, connectedClientSocket));

                    IAuthenticationHandler authenticationHandler = scope.ServiceProvider.GetRequiredService<IAuthenticationHandler>();

                    if (metaData.Type == MessageType.RegistrationRequest)
                        await authenticationHandler.RegisterAsync(message, _cancellationToken).ConfigureAwait(false);
                    else
                    {
                        IClientInfo client = await authenticationHandler.Authenticate(message, _cancellationToken).ConfigureAwait(false);
                        this.RegisterConnectedUser(client);
                        this.ListenForMessagesAsync(client);
                    }
                }
                catch (Exception ex)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        _logger.LogWarning("Listening caceled while receiving frame from: {0}", ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());
                    else
                    {
                        _logger.LogError(ex, "Exception occured while receiving and creating frame metadata from: {0}. Check Logs for more info!",
                            ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());
                    }

                    await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(connectedClientSocket, ex, _cancellationToken);
                }
            }
        }

        private void RegisterConnectedUser(IClientInfo clientInfo)
        {
            bool result = _connectedClients.TryAdd(clientInfo.Name, clientInfo);

            if (!result)
            {
                _logger.LogError("Failed to add user with ID: {0} to connected clients collection.", clientInfo.Id);
                throw new Exception($"Failed to add user with ID: {clientInfo.Id} to connected clients collection of type {_connectedClients.GetType().Name}.");
            }
        }

        private async void ListenForMessagesAsync(IClientInfo clientInfo)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                while (!_cancellationToken.IsCancellationRequested && clientInfo.Socket.Connected)
                {
                    try
                    {
                        byte[] frameMetaData = await this.ReceiveDataAsync(clientInfo.Socket, _frameMetaDataConfiguration.MetaDataFieldsTotalSize).ConfigureAwait(false);
                        IFrameMetaData frameMeta = scope.ServiceProvider.GetRequiredService<IFrameMetaEncoder>().Decode(frameMetaData);

                        byte[] data = await this.ReceiveDataAsync(clientInfo.Socket, frameMeta.HeadersDataLength + frameMeta.MessageDataLength).ConfigureAwait(false);
                        IMessage message = scope.ServiceProvider.GetRequiredService<IMessageEncoder>().Decode(data, frameMeta, clientInfo);

                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().DispatchAsync(message, _connectedClients, _cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception occured while listening for messages from {0} ({1}).", clientInfo.Name, clientInfo.RemoteEndPoint.ToString());
                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(clientInfo, ex, _cancellationToken);
                    }
                }
            }

            clientInfo.Socket.Close();
            _connectedClients.TryRemove(clientInfo.Name, out IClientInfo result);
            _logger.LogInformation($"User: {result.Name} DISCONNECTED.");
        }


        private async Task<byte[]> ReceiveDataAsync(Socket clientSocket, int dataLength)
        {
            if (_cancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<byte[]>(_cancellationToken);

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[dataLength]);
            try
            {
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None, _cancellationToken);
            }
            catch (AggregateException ex)
            {
                _logger.LogError(ex, "Data receiving from {0} has to stopped due to exception: {1}",
                        ((IPEndPoint)clientSocket?.RemoteEndPoint)?.Address.ToString(), ex.Message);
                return await Task.FromException<byte[]>(ex);
            }

            return buffer.Array;
        }
    }
}
