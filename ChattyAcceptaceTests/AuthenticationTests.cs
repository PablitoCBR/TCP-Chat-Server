using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace ChattyAcceptaceTests
{
    public class AuthenticationTests : RunningServerTestTemplate
    {
        [Fact]
        public void AuthenticateRegistaredUserTest()
        {
            Socket clientSocket = CreateClientSocketConnectedToServer();

            bool registerResult = TryRegisterUser(clientSocket, "admin", "admin");
            Assert.True(registerResult);

            clientSocket.Dispose();
            clientSocket = CreateClientSocketConnectedToServer();
            bool authenticationResult = TryAuthenticateUser(clientSocket, "admin", "admin");
            Assert.True(authenticationResult);
        }

        [Fact]
        public void AuthenticateNotRegisteredUserTest()
        {
            Socket clientSocket = CreateClientSocketConnectedToServer();
            bool authenticationResult = TryAuthenticateUser(clientSocket, "user", "password");
            Assert.False(authenticationResult);
        }
    }
}
