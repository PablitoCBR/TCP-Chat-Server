using System;

namespace ChattyAcceptaceTests
{
    public abstract class RunningServerTestTemplate : TestTemplate, IDisposable
    {
        protected RunningServerTestTemplate()
            : base()
        {
            StartHostServerAsync().Wait();
        }

        public void Dispose() => StopHostServerAsync().Wait();
    }
}
