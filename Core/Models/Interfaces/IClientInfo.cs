using System.Net;
using System.Net.Sockets;

namespace Core.Models.Interfaces
{
    public interface IClientInfo
    {
        int Id { get; }

        string Name { get; }

        Socket Socket { get; }

        IPEndPoint RemoteEndPoint { get; }
    }
}
