using System.Net;
using System.Net.Sockets;

namespace Core.Models
{
    public class ClientInfo
    {
        public int Id { get; }

        public string Name { get; }

        public Socket Socket { get; }

        public IPEndPoint RemoteEndPoint { get; }

        private ClientInfo(int id, string name, Socket socket)
        {
            Id = id;
            Name = name;
            Socket = socket;
            RemoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
        }

        public static ClientInfo Create(int id, string name, Socket socket)
            => new ClientInfo(id, name, socket);
    }
}
