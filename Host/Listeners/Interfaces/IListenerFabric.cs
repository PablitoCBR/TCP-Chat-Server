﻿
namespace Host.Listeners.Interfaces
{
    public interface IListenerFabric
    {
        IListener CreateTcpListener(int port, int pendingConnectionsLimit);
    }
}
