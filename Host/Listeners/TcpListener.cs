using Core.Handlers.Security.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Models.Exceptions.UserFaultExceptions;
using Core.Pipeline;
using Core.Services.Encoders;
using Host.Listeners.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        private readonly ILogger<IListener> _logger;

        private readonly IServiceProvider _serviceProvider;

        private readonly ListenerSettings _listenerSettings;

        private readonly IPEndPoint _ipEndPoint;

        private readonly ConcurrentDictionary<string, ClientInfo> _connectedClients;

        private readonly FrameMetaDataConfiguration _frameMetaDataConfiguration;

        private readonly ManualResetEvent _acceptEvent;

        private CancellationToken _cancellationToken;

        public TcpListener(ListenerSettings settings, IPEndPoint ipEndPoint, ILogger<IListener> logger,
            IOptions<FrameMetaDataConfiguration> frameMetaDataConfiguration, IServiceProvider serviceProvider)
        {
            ProtocolType = ProtocolType.Tcp;
            IsListening = false;

            _connectedClients = new ConcurrentDictionary<string, ClientInfo>();

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

            try
            {
                _cancellationToken.Register(() => _acceptEvent.Set());

                while (!_cancellationToken.IsCancellationRequested)
                {
                    _acceptEvent.Reset();
                    listener.BeginAccept(new AsyncCallback(HandleConnectionAttemptAsync), listener);
                    _acceptEvent.WaitOne();
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("TCP Listener was closed.");

                foreach (var pair in _connectedClients)
                {
                    _connectedClients.TryRemove(pair.Key, out ClientInfo clientInfo);
                    clientInfo?.Socket.Disconnect(false);
                    clientInfo?.Socket.Dispose();
                }
            }
            finally
            {
                listener.Close();
                IsListening = false;
            }
        }

        private async void HandleConnectionAttemptAsync(IAsyncResult asyncResult)
        {
            _acceptEvent.Set();

            if (_cancellationToken.IsCancellationRequested)
                return;

            Socket connectedClientSocket = ((Socket)asyncResult.AsyncState).EndAccept(asyncResult);
            _logger.LogInformation("Connection accepted from: {0}", ((IPEndPoint)connectedClientSocket.RemoteEndPoint).Address.ToString());

            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    byte[] frameMetaData = await ReceiveDataAsync(connectedClientSocket, _frameMetaDataConfiguration.MetaDataFieldsTotalSize);
                    FrameMetaData metaData = scope.ServiceProvider.GetRequiredService<IFrameMetaEncoder>().Decode(frameMetaData);

                    if (!(metaData.Type == MessageType.RegistrationRequest || metaData.Type == MessageType.AuthenticationRequest))
                        throw new InvalidMessageException(metaData.Type, "Only registration and authentication requests allowed.");


                    byte[] data = await this.ReceiveDataAsync(connectedClientSocket, metaData.HeadersDataLength + metaData.MessageDataLength).ConfigureAwait(false);
                    Message message = scope.ServiceProvider.GetRequiredService<IMessageEncoder>().Decode(data, metaData, ClientInfo.Create(0, string.Empty, connectedClientSocket));

                    IAuthenticationHandler authenticationHandler = scope.ServiceProvider.GetRequiredService<IAuthenticationHandler>();

                    if (metaData.Type == MessageType.RegistrationRequest)
                        await authenticationHandler.RegisterAsync(message, _cancellationToken).ConfigureAwait(false);
                    else
                    {
                        ClientInfo client = await authenticationHandler.Authenticate(message, _cancellationToken).ConfigureAwait(false);
                        this.RegisterConnectedUser(client);
                        Task.Factory.StartNew(() => ListenForMessagesAsync(client), TaskCreationOptions.AttachedToParent).Unwrap();
                    }
                }
                catch (Exception ex)
                {
                    if(!_cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogError(ex, "Exception occured on registration/authentication attempt from {0}. Check Logs for more info!",
                            ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());

                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(connectedClientSocket, ex, _cancellationToken);
                    }
                }
            }
        }

        private void RegisterConnectedUser(ClientInfo clientInfo)
        {
            bool result = _connectedClients.TryAdd(clientInfo.Name, clientInfo);

            if (!result)
            {
                _logger.LogError("Failed to add user with ID: {0} to connected clients collection.", clientInfo.Id);
                throw new Exception($"Failed to add user with ID: {clientInfo.Id} to connected clients collection of type {_connectedClients.GetType().Name}.");
            }
        }

        private async Task ListenForMessagesAsync(ClientInfo clientInfo)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                while (!_cancellationToken.IsCancellationRequested && clientInfo.Socket.Connected)
                {
                    try
                    {
                        byte[] frameMetaData = await this.ReceiveDataAsync(clientInfo.Socket, _frameMetaDataConfiguration.MetaDataFieldsTotalSize).ConfigureAwait(false);
                        FrameMetaData frameMeta = scope.ServiceProvider.GetRequiredService<IFrameMetaEncoder>().Decode(frameMetaData);

                        byte[] data = await this.ReceiveDataAsync(clientInfo.Socket, frameMeta.HeadersDataLength + frameMeta.MessageDataLength).ConfigureAwait(false);
                        Message message = scope.ServiceProvider.GetRequiredService<IMessageEncoder>().Decode(data, frameMeta, clientInfo);

                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().DispatchAsync(message, _connectedClients, _cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            _logger.LogInformation("Listening from {0} canceled.", clientInfo.Name);
                        else await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(clientInfo, ex, _cancellationToken);
                    }
                }
            }

            if (clientInfo.Socket.Connected)
                clientInfo.Socket.Disconnect(false);

            clientInfo.Socket.Close();
            _connectedClients.TryRemove(clientInfo.Name, out ClientInfo result);
            _logger.LogInformation($"User: {result.Name} DISCONNECTED.");
            _cancellationToken.ThrowIfCancellationRequested();
        }


        private async Task<byte[]> ReceiveDataAsync(Socket clientSocket, int dataLength)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (dataLength <= 0)
                return Array.Empty<byte>();

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
