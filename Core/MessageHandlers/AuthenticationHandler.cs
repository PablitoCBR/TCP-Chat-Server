using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.MessageHandlers.Interfaces;
using Core.Models;
using Core.Models.Interfaces;
using Core.Security.Interfaces;
using DAL.Models;
using DAL.Repositories.Interfaces;

namespace Core.MessageHandlers
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private ISecurityService SecurityService { get; }

        private IUserRepository UserRepository { get; }

        public AuthenticationHandler(ISecurityService securityService, IUserRepository userRepository)
        {
            this.SecurityService = securityService;
            this.UserRepository = userRepository;
        }

        public async Task<IClientInfo> Authenticate(IMessage message)
        {
            var (username, password) = this.GetCredentials(message.Headers);
            User user = await this.UserRepository.GetByNameAsync(username);

            if (user == null)
                throw new Exception("no user");

            if (!this.SecurityService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                throw new Exception("invalid password");

            return ClientInfo.Create(user.Id, user.Username, message.ClientInfo.Socket);
        }

        public async Task RegisterAsync(IMessage message)
        {
            var (username, password) = this.GetCredentials(message.Headers);
            var (passwordHash, salt) = this.SecurityService.GenerateHash(password);

            await this.UserRepository.AddAsync(new User(username, passwordHash, salt, DateTime.Now, message.ClientInfo.RemoteEndPoint.ToString()));
        }

        private (string username, string password) GetCredentials(IDictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authentication"))
                throw new ArgumentException("No authentication header");

            string credentialsBase64 = headers["Authentication"];
            string credentailsString = Encoding.ASCII.GetString(Convert.FromBase64String(credentialsBase64));
            string[] credentials = credentailsString.Split(':');
            return (credentials[0], credentials[1]);
        }
    }
}
