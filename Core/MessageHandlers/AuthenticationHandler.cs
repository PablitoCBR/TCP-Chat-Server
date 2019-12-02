using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.MessageHandlers.Interfaces;
using Core.Models;
using Core.Models.Exceptions.UserFaultExceptions;
using Core.Models.Enums;
using Core.Models.Interfaces;
using Core.Security.Interfaces;
using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.MessageHandlers
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private ISecurityService SecurityService { get; }

        private IUserRepository UserRepository { get; }

        private ILogger<IAuthenticationHandler> Logger { get; }

        public AuthenticationHandler(ISecurityService securityService, IUserRepository userRepository, ILogger<IAuthenticationHandler> logger)
        {
            this.SecurityService = securityService;
            this.UserRepository = userRepository;
            this.Logger = logger;
        }

        public async Task<IClientInfo> Authenticate(IMessage message)
        {
            var (username, password) = this.GetCredentials(message.Headers);
            User user = await this.UserRepository.GetByNameAsync(username);

            if (user == null)
                throw new AuthenticationException(MessageType.Unauthenticated, "Invalid username or password.");

            if (!this.SecurityService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                this.Logger.LogWarning("Failed authentication attempt for user: {0}, from IP: {1}", username, message.ClientInfo.RemoteEndPoint);
                throw new AuthenticationException(MessageType.Unauthenticated, "Invalid username or password.");
            }
                

            this.Logger.LogInformation("User {0} was authenticated successfully.", username);
            return ClientInfo.Create(user.Id, user.Username, message.ClientInfo.Socket);
        }

        public async Task RegisterAsync(IMessage message)
        {
            var (username, password) = this.GetCredentials(message.Headers);

            if (await this.UserRepository.AnyWithNameAsync(username))
                throw new AuthenticationException(MessageType.UsernameAlreadyTaken, "Username is already taken.");

            var (passwordHash, salt) = this.SecurityService.GenerateHash(password);

            await this.UserRepository.AddAsync(new User(username, passwordHash, salt, DateTime.Now, message.ClientInfo.RemoteEndPoint.ToString()));
            this.Logger.LogInformation("Registered new user. Username: {0}, Connected from IP: {1}.", username, message.ClientInfo.RemoteEndPoint);
        }

        private (string username, string password) GetCredentials(IDictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authentication"))
                throw new AuthenticationException(MessageType.InvalidAuthentication, "Authentication header was not found.");

            string credentialsBase64 = headers["Authentication"];
            string credentailsString = Encoding.ASCII.GetString(Convert.FromBase64String(credentialsBase64));
            string[] credentials = credentailsString.Split(':');
            return (credentials[0], credentials[1]);
        }
    }
}
