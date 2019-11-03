using System.Net.Sockets;

namespace Host.Models.Interfaces
{
    public interface IClientInfo
    {
        int Id { get; }

        string Name { get; }

        Socket Socket { get; }
    }
}
