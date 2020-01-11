﻿using Host.Abstractions;
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

        public bool IsActive { get; private set; }

        public Host(IListener listener, ILogger<IHost> logger)
        {
            _tcpListener = listener;
            _logger = logger;
            IsActive = false;
        }

        public void Run()
        {
            if (IsActive)
            {
                _logger.LogError("Listener is already runnin.");
            }
            else
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _logger.LogInformation("Starting TCP Listerner.");

                Task.Factory.StartNew(
                    () => _tcpListener.Listen(_cancellationTokenSource.Token),
                    _cancellationTokenSource.Token);

                IsActive = true;
            }
        }

        public void Reset()
        {
            _logger.LogInformation("Restarting listener.");
            this.Stop();
            this.Run();
            _logger.LogInformation("Listener restrted.");
        }

        public void Stop()
        {
            if (IsActive == false)
            {
                _logger.LogError("Listener is not running.");
            }
            else
            {
                _logger.LogInformation("Stoping listener.");

                _cancellationTokenSource.Cancel();
                IsActive = false;

                _logger.LogInformation("Listening stopped.");
            }
        }
    }
}
