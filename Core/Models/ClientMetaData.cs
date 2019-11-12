﻿using System.Net;
using System.Net.Sockets;
using Core.Models.Interfaces;

namespace Core.Models
{
    public class ClientMetaData : IClientInfo
    {
        public int Id { get; }

        public string Name { get; }

        public Socket Socket { get; }

        public IPEndPoint RemoteEndPoint { get; }

        private ClientMetaData(int id, string name, Socket socket)
        {
            Id = id;
            Name = name;
            Socket = socket;
            RemoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
        }

        public static IClientInfo Create(int id, string name, Socket socket)
            => new ClientMetaData(id, name, socket);
    }
}
