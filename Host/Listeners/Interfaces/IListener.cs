using System;
using System.Net.Sockets;
using System.Threading;

namespace Host.Listeners.Interfaces
{
    public interface IListener : IDisposable
    {
        ProtocolType ProtocolType { get; }

        bool IsListening { get; }

        void Listen(CancellationToken cancellationToken);
    }
}
