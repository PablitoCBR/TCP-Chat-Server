using Host.Abstractions;
using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Host
{
    public class Host : IHost
    {
        private readonly IListener _tcpListener;

        private readonly ILogger<IHost> _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _isActive;
        
        public Host(IListener listener, ILogger<IHost> logger)
        {
            this._tcpListener = listener;
        }

        public void Run()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            //this._logger.LogInformation("Starting TCP Listerner.");
            Task.Factory.StartNew(() => this._tcpListener.Listen(this._cancellationTokenSource.Token), this._cancellationTokenSource.Token);
            this._isActive = true;
            while (true) { }
        }

        public void Reset()
        {
        }

        public void Stop()
        {
        }
    }
}
