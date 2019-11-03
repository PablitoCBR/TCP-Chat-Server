using Host.Abstractions;
using Host.Listeners.Interfaces;
using System.Threading;

namespace Host
{
    public class Host : IHost
    {
        private IListener TcpListener { get; }

        public Host(IListener listener)
        {
            TcpListener = listener;
        }

        public void Run()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            TcpListener.Listen(tokenSource.Token);
        }
    }
}
