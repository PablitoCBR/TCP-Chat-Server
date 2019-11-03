using System.Net.Sockets;
using Host.Models.Interfaces;

namespace Host.Models
{
    public class ClientMetaData : IClientInfo
    {
        public int Id { get; }

        public string Name { get; }

        public Socket Socket { get; }

        private ClientMetaData(int id, string name, Socket socket)
        {
            Id = id;
            Name = name;
            Socket = socket;
        }

        public static IClientInfo Create(int id, string name, Socket socket)
            => new ClientMetaData(id, name, socket);
    }
}
