namespace Core.Security
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Core.Models;
    using Core.Models.Exceptions.UserFaultExceptions;
    using Core.Models.Enums;
    using Core.Models.Interfaces;

    using Core.Services.Security.Interfaces;
    using Core.Services.Factories.Interfaces;
    using Core.Security.Interfaces;

    using DAL.Models;
    using DAL.Repositories.Interfaces;

    using Microsoft.Extensions.Logging;

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

        public async Task<IClientInfo> Authenticate(IMessage message) 
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
            message.ClientInfo.Socket.Send(_messageFactory.CreateBytes(message.ClientInfo, MessageType.Authenticated, new Dictionary<string, string>())); // specify headers

            return ClientInfo.Create(user.Id, user.Username, message.ClientInfo.Socket);
        }

        public async Task RegisterAsync(IMessage message) 
        {
            var (username, password) = this.GetCredentials(message.Headers);

            if (await _userRepository.AnyWithNameAsync(username))
                throw new AuthenticationException(MessageType.RegistrationFailed, "Username is already taken.");

            var (passwordHash, salt) = _securityService.GenerateHash(password);

            await _userRepository.AddAsync(new User(username, passwordHash, salt, DateTime.Now, message.ClientInfo.RemoteEndPoint.ToString()));
            _logger.LogInformation("Registered new user. Username: {0}, Connected from IP: {1}.", username, message.ClientInfo.RemoteEndPoint);
            message.ClientInfo.Socket.Send(_messageFactory.CreateBytes(message.ClientInfo, MessageType.Registered, new Dictionary<string, string>())); 
        }

        private (string username, string password) GetCredentials(IDictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authentication"))
                throw new AuthenticationException(MessageType.Unauthenticated, "Authentication header was not found.");

            string credentialsBase64 = headers["Authentication"];
            string credentailsString = Encoding.ASCII.GetString(Convert.FromBase64String(credentialsBase64));
            string[] credentials = credentailsString.Split(':');
            return (credentials[0], credentials[1]);
        }
    }
}
