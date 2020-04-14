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

namespace Core.Handlers.Security
{
    public class RegistrationHandler : AbstractSecurityHandler, IRegistrationHandler
    {
        private readonly ISecurityService _securityService;

        private readonly IUserRepository _userRepository;

        private readonly ILogger<IRegistrationHandler> _logger;

        private readonly IMessageFactory _messageFactory;

        public RegistrationHandler(ISecurityService securityService, IUserRepository userRepository, ILogger<IRegistrationHandler> logger, IMessageFactory messageFactory)
        {
            _securityService = securityService;
            _userRepository = userRepository;
            _logger = logger;
            _messageFactory = messageFactory;
        }

        public async Task RegisterAsync(Message message, CancellationToken cancellationToken)
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

    }

    public interface IRegistrationHandler
    {
        Task RegisterAsync(Message messasge, CancellationToken cancellationToken = default(CancellationToken));
    }
}
