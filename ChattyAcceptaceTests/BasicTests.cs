using System.Threading.Tasks;
using Xunit;

namespace ChattyAcceptaceTests
{
    public class BasicTests : TestTemplate
    {
        [Fact]
        public async Task RunServerHostTest()
        {
            await StartHostServerAsync();
            Assert.True(Host.IsActive);

            await StopHostServerAsync();
        }

        [Fact]
        public async Task StopServerHostTest()
        {
            await StartHostServerAsync();
            Assert.True(Host.IsActive);

            await StopHostServerAsync();
            Assert.False(Host.IsActive);
        }

        [Fact]
        public async Task StartStopServerHostTest()
        {
            await StartHostServerAsync();
            Assert.True(Host.IsActive);

            await StopHostServerAsync();
            Assert.False(Host.IsActive);
        }
    }
}
