using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace ChattyAcceptaceTests
{
    public class RegisterTest : RunningServerTestTemplate
    {
        [Fact]
        public void RegisterUserTest()
        {
            Socket clientSocket = CreateClientSocketConnectedToServer();
            bool result = TryRegisterUser(clientSocket, "admin", "admin");

            Assert.True(result);
        }

        [Fact]
        public void RegisterTwoUsersWithSameNameTest()
        {
            Socket firstClientSocket = CreateClientSocketConnectedToServer();
            bool resultFirst = TryRegisterUser(firstClientSocket, "redundant", "admin");
            Assert.True(resultFirst);

            Socket secondClientSocket = CreateClientSocketConnectedToServer();
            bool resultSecond = TryRegisterUser(secondClientSocket, "redundant", "admin");
            Assert.False(resultSecond);
        }

        [Fact]
        public async Task RegisterUserAfterHostRestartTest()
        {
            await StopHostServerAsync();
            Assert.False(Host.IsActive);
            await StartHostServerAsync();
            Assert.True(Host.IsActive);

            Socket clientSocket = CreateClientSocketConnectedToServer();
            bool result = TryRegisterUser(clientSocket, "afterRestart", "admin");

            Assert.True(result);
        }

        [Fact]
        public async Task RegisterTwoUsersWithSameNameAfterHostRestartTest()
        {
            await StopHostServerAsync();
            Assert.False(Host.IsActive);
            await StartHostServerAsync();
            Assert.True(Host.IsActive);

            Socket firstClientSocket = CreateClientSocketConnectedToServer();
            bool resultFirst = TryRegisterUser(firstClientSocket, "afterRestartRedundand", "admin");
            Assert.True(resultFirst);

            Socket secondClientSocket = CreateClientSocketConnectedToServer();
            bool resultSecond = TryRegisterUser(secondClientSocket, "afterRestartRedundand", "admin");
            Assert.False(resultSecond);
        }
    }
}
