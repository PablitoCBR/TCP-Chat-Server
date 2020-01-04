namespace Core.Handlers.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    using Core.Models;
    using Core.Models.Consts;
    using Core.Models.Exceptions.UserFaultExceptions;
    using Core.Models.Enums;
    using Core.Models.Interfaces;

    using Core.Services.Security.Interfaces;
    using Core.Services.Factories.Interfaces;

    using Core.Handlers.Security.Interfaces;

    using DAL.Models;
    using DAL.Repositories.Interfaces;

    using Microsoft.Extensions.Logging;
    using System.Threading;

    public class AuthenticationHandler : IAuthenticationHandler
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

        public async Task<IClientInfo> Authenticate(IMessage message, CancellationToken cancellationToken) 
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
            IClientInfo clientInfo = ClientInfo.Create(user.Id, user.Username, message.ClientInfo.Socket);

            user.LastConnected = DateTime.Now;
            if (!String.Equals(clientInfo.RemoteEndPoint.Address.ToString(), user.LastKnownIPAddress))
                user.LastKnownIPAddress = clientInfo.RemoteEndPoint.Address.ToString();

            return clientInfo;
        }

        public async Task RegisterAsync(IMessage message, CancellationToken cancellationToken) 
        {
            var (username, password) = this.GetCredentials(message.Headers);

            if (await _userRepository.AnyWithNameAsync(username))
                throw new AuthenticationException(MessageType.RegistrationFailed, "Username is already taken.");

            var (passwordHash, salt) = _securityService.GenerateHash(password);

            await _userRepository.AddAsync(new User(username, passwordHash, salt, DateTime.Now, message.ClientInfo.RemoteEndPoint.ToString()));

            _logger.LogInformation("Registered new user. Username: {0}, Connected from IP: {1}.", username, message.ClientInfo.RemoteEndPoint);

            byte[] confirmationMessage = _messageFactory.CreateBytes(MessageType.Registered);
            await message.ClientInfo.Socket.SendAsync(new ArraySegment<byte>(confirmationMessage), SocketFlags.None, cancellationToken); 
        }

        private (string username, string password) GetCredentials(IDictionary<string, string> headers)
        {
            if (!headers.ContainsKey(MessageHeaders.Authentication))
                throw new AuthenticationException(MessageType.Unauthenticated, "Authentication header was not found.");

            string credentialsBase64 = headers[MessageHeaders.Authentication];
            string credentailsString = Encoding.ASCII.GetString(Convert.FromBase64String(credentialsBase64));
            string[] credentials = credentailsString.Split(':');
            return (credentials[0], credentials[1]);
        }
    }
}
