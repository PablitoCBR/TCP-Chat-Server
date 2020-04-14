namespace Core.Handlers.Security
{
    using Core.Models;
    using Core.Models.Enums;
    using Core.Models.Exceptions.UserFaultExceptions;
    using Core.Services.Factories;
    using Core.Services.Security;
    using DAL.Models;
    using DAL.Repositories.Interfaces;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class AuthenticationHandler : AbstractSecurityHandler, IAuthenticationHandler
    {
        private readonly ISecurityService _securityService;

        private readonly IUserRepository _userRepository;

        private readonly ILogger<IAuthenticationHandler> _logger;

        private readonly IMessageFactory _messageFactory;

        public AuthenticationHandler(ISecurityService securityService, IUserRepository userRepository, ILogger<IAuthenticationHandler> logger, IMessageFactory messageFactory)
        {
            _securityService = securityService;
            _userRepository = userRepository;
            _logger = logger;
            _messageFactory = messageFactory;
        }

        public async Task<ClientInfo> Authenticate(Message message, CancellationToken cancellationToken)
        {
            var (username, password) = this.GetCredentials(message.Headers);
            User user = await _userRepository.GetByNameAsync(username);

            if (user == null)
                throw new AuthenticationException(MessageType.Unauthenticated, "Invalid username or password.");

            if (!_securityService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Failed authentication attempt for user: {0}, from IP: {1}", username, message.ClientInfo.RemoteEndPoint);
                throw new AuthenticationException(MessageType.Unauthenticated, "Invalid username or password.");
            }

            _logger.LogInformation("User {0} was authenticated successfully.", username);

            byte[] responseMessage = _messageFactory.CreateBytes(MessageType.Authenticated);
            await message.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(responseMessage), SocketFlags.None, cancellationToken);
            ClientInfo clientInfo = ClientInfo.Create(user.Id, user.Username, message.ClientInfo.Socket);

            user.LastConnected = DateTime.Now;
            if (!String.Equals(clientInfo.RemoteEndPoint.Address.ToString(), user.LastKnownIPAddress))
                user.LastKnownIPAddress = clientInfo.RemoteEndPoint.Address.ToString();

            return clientInfo;
        }
    }

    public interface IAuthenticationHandler
    {
        Task<ClientInfo> Authenticate(Message message, CancellationToken cancellationToken);
    }
}
