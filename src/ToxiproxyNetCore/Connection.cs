using System;

namespace Toxiproxy.Net
{
    /// <summary>
    /// The class to connect to the ToxiProxy server
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Connection : IDisposable
    {
        private const int _defaultListeningPort = 8474;

        private readonly IHttpClientFactory _clientFactory;
        private readonly bool _resetAllToxicsAndProxiesOnClose;
        
        public Connection(bool resetAllToxicsAndProxiesOnClose = false)
            : this("127.0.0.1", resetAllToxicsAndProxiesOnClose)
        {
            // Nothing here   
        }

        public Connection(string host, bool resetAllToxicsAndProxiesOnClose = false)
            : this(host, _defaultListeningPort, resetAllToxicsAndProxiesOnClose)
        {
            // Nothing here
        }

        public Connection(string host, int port, bool resetAllToxicsAndProxiesOnClose = false)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host");
            }
            _resetAllToxicsAndProxiesOnClose = resetAllToxicsAndProxiesOnClose;
            _clientFactory = new HttpClientFactory(new Uri($"http://{host}:{port}"));
        }

        public Client Client() => new (_clientFactory);

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_resetAllToxicsAndProxiesOnClose)
                    {
                        Client().ResetAsync().Wait();
                    }
                }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Connection()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        //public void Dispose()
        //{
        //    if (_resetAllToxicsAndProxiesOnClose)
        //    {
        //        Client().Reset();
        //    }
        //}
    }
}
