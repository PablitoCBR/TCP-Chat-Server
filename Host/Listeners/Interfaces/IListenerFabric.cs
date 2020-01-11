
using Microsoft.Extensions.Options;

namespace Host.Listeners.Interfaces
{
    public interface IListenerFabric
    {
        IListener CreateTcpListener(int port, IOptions<ListenerSettings> settings);
    }
}
