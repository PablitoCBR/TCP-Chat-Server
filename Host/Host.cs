using Host.Abstractions;
using Host.Listeners.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Host
{
    public class Host : IHost
    {
        private readonly IListener _tcpListener;

        private readonly ILogger<IHost> _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _isActive;

        private Thread _listeningThread;

        public Host(IListener listener, ILogger<IHost> logger)
        {
            this._tcpListener = listener;
        }

        public void Run()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            this._listeningThread = new Thread(() => this._tcpListener.Listen(this._cancellationTokenSource.Token));
            this._listeningThread.Start();
            this._isActive = true;
            while(true)
            {

            }
        }

        public void Reset()
        {
            if(this._isActive && this._listeningThread.IsAlive)
            {
                this._cancellationTokenSource.Cancel();
                this._isActive = false;

                try
                {
                    this._listeningThread.Abort();
                }
                catch(ThreadAbortException)
                {
                    this._logger.LogWarning("Lsitening thread aborted due to reset request!");
                }

                this._cancellationTokenSource.Dispose();
                this.Run();
                this._logger.LogInformation("Host is listening.");
                return;
            }

            else if(this._isActive && !this._listeningThread.IsAlive)
            {
                this._logger.LogCritical("Host indicate active state but listening thread is not alive! Thread: {0}", this._listeningThread.Name);
                this._logger.LogCritical("Performing HARD reset...");

                this._cancellationTokenSource.Cancel();

                try
                {
                    this._listeningThread.Abort();
                }
                catch(ThreadAbortException)
                {
                    this._logger.LogCritical("Thread aborted...");
                }

                this._cancellationTokenSource.Dispose();
                this.Run();
                this._logger.LogWarning("Host is listening after HARD reset.");
            }

            else this._logger.LogWarning("Attempt to restart disabled host!");
        }

        public void Stop()
        {
            if(this._isActive && this._listeningThread.IsAlive)
            {
                this._logger.LogInformation("Stopping host...");
                this._cancellationTokenSource.Cancel();

                try
                {
                    this._listeningThread.Abort();
                }
                catch (ThreadAbortException)
                {
                    this._logger.LogWarning("Lsitening thread aborted due to stop request!");
                }

                this._isActive = false;
                this._logger.LogInformation("Host stopped.");
            }

            else if (this._isActive && !this._listeningThread.IsAlive)
            {
                this._logger.LogCritical("Host indicate active state but listening thread is not alive! Thread: {0}", this._listeningThread.Name);
                this._logger.LogCritical("Performing HARD stop...");

                this._cancellationTokenSource.Cancel();

                try
                {
                    this._listeningThread.Abort();
                }
                catch (ThreadAbortException)
                {
                    this._logger.LogCritical("Thread aborted...");
                }

                this._cancellationTokenSource.Dispose();
                this._logger.LogWarning("Host fully stopped.");
                
            }

            else this._logger.LogWarning("Attempt to stop disabled host!");
        }
    }
}
