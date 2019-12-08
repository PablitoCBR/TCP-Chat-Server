using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Host.Listeners.Interfaces;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Core.Models.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Microsoft.Extensions.Options;
using Core.Models.Exceptions.UserFaultExceptions;
using Core.Services.Encoders.Interfaces;
using Core.MessageHandlers.Interfaces;
using Core.Pipeline.Interfaces;

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


        public TcpListener(ListennerSettings settings, IPEndPoint ipEndPoint, ILogger<IListener> logger, IOptions<FrameMetaDataConfiguration> frameMetaDataConfiguration, IServiceProvider serviceProvider)
        {
            ProtocolType = ProtocolType.Tcp;
            IsListening = false;

            _connectedClients = new ConcurrentDictionary<string, IClientInfo>();

            _logger = logger;
            _listenerSettings = settings;
            _ipEndPoint = ipEndPoint;
            _frameMetaDataConfiguration = frameMetaDataConfiguration.Value;

            _serviceProvider = serviceProvider;

            _logger.LogInformation("TCP Listener created and assigned to port: {0}", _ipEndPoint.Port);
        }

        public void Listen(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting listening for TCP transimssions on port: {0}, IP: {1}", _ipEndPoint.Port, _ipEndPoint.Address.ToString());

            Socket listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType);
            listener.Bind(_ipEndPoint);
            listener.Listen(_listenerSettings.PendingConnectionsQueue);
            IsListening = true;

            while (!cancellationToken.IsCancellationRequested)
            {
                Task<Socket> acceptTask = listener.AcceptAsync();
                this.HandleConnectionAttemptAsync(acceptTask, cancellationToken).ConfigureAwait(false);
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

        private async Task HandleConnectionAttemptAsync(Task<Socket> acceptTask, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            Socket connectedClientSocket = await acceptTask;
            _logger.LogInformation("Connection accepted from: {0}", ((IPEndPoint)connectedClientSocket.RemoteEndPoint).Address.ToString());

            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    byte[] frameMetaData = await ReceiveDataAsync(connectedClientSocket, _frameMetaDataConfiguration.MetaDataLength, cancellationToken);
                    IFrameMetaData metaData = scope.ServiceProvider.GetRequiredService<IFrameMetaEncoder>().Decode(frameMetaData);

                    if (!(metaData.Type == MessageType.RegistrationRequest || metaData.Type == MessageType.AuthenticationRequest))
                    {
                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(
                             connectedClientSocket,
                             new InvalidMessageException(metaData.Type, "Only registration and authentication requests allowed."));
                    }
                    else
                    {
                        byte[] data = await this.ReceiveDataAsync(connectedClientSocket, metaData.HeadersDataLength + metaData.MessageDataLength, cancellationToken).ConfigureAwait(false);
                        IMessage message = scope.ServiceProvider.GetRequiredService<IMessageEncoder>().Decode(data, metaData, ClientInfo.Create(0, string.Empty, connectedClientSocket));

                        if (metaData.Type == MessageType.RegistrationRequest)
                        {
                            await scope.ServiceProvider.GetRequiredService<IAuthenticationHandler>().RegisterAsync(message);
                        }
                        else
                        {
                            IClientInfo client = await scope.ServiceProvider.GetRequiredService<IAuthenticationHandler>().Authenticate(message).ConfigureAwait(false);
                            this.RegisterConnectedUser(client);
                            this.ListenForMessagesAsync(client, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                        _logger.LogWarning("Listening caceled while receiving frame from: {0}", ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());
                    else
                    {
                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(connectedClientSocket, ex);

                        _logger.LogError(ex, "Exception occured while receiving and creating frame metadata from: {0}. Check Logs for more info!",
                            ((IPEndPoint)connectedClientSocket?.RemoteEndPoint)?.Address.ToString());
                    }

                    connectedClientSocket.Close();
                    return;
                }
            }
        }

        private void RegisterConnectedUser(IClientInfo clientInfo)
        {
            bool result = _connectedClients.TryAdd(clientInfo.Name, clientInfo);
            if (!result) _logger.LogError("Failed to add user with ID: {0} to ConnectedClients collection", clientInfo.Id);
        }

        private async void ListenForMessagesAsync(IClientInfo clientInfo, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        byte[] frameMetaData = await this.ReceiveDataAsync(clientInfo.Socket, _frameMetaDataConfiguration.MetaDataLength, cancellationToken).ConfigureAwait(false);
                        IFrameMetaData frameMeta = scope.ServiceProvider.GetRequiredService<IFrameMetaEncoder>().Decode(frameMetaData);

                        byte[] data = await this.ReceiveDataAsync(clientInfo.Socket, frameMeta.HeadersDataLength + frameMeta.MessageDataLength, cancellationToken).ConfigureAwait(false);
                        IMessage message = scope.ServiceProvider.GetRequiredService<IMessageEncoder>().Decode(data, frameMeta, clientInfo);

                        await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().DispatchAsync(message, _connectedClients).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (ex is SocketException)
                        {
                            _logger.LogInformation("Connection with: {0} was closed by user.", clientInfo.Name);
                            break;
                        }
                        else
                        {
                            await scope.ServiceProvider.GetRequiredService<IMessageDispatcher>().OnExceptionAsync(clientInfo, ex); // <- jezeli wyjatek był krytycznie handler zamknie socket co zostanei przechwycone przy kolejnej iteracji
                            _logger.LogError(ex, "Exception occured while listening for messages from {0} ({1}).", clientInfo.Name, clientInfo.RemoteEndPoint.ToString());
                        }
                    }
                }
            }

            clientInfo.Socket.Close();
            _connectedClients.TryRemove(clientInfo.Name, out IClientInfo result);
            _logger.LogInformation($"User: {result.Name} DISCONNECTED.");
        }


        private async Task<byte[]> ReceiveDataAsync(Socket clientSocket, int dataLength, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<byte[]>(cancellationToken);

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[dataLength]);
            try
            {
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
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
