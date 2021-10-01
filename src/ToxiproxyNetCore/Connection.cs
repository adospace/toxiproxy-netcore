using System;
using System.Threading.Tasks;

namespace Toxiproxy.Net
{
    /// <summary>
    /// The class to connect to the ToxiProxy server
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Connection : IDisposable, IAsyncDisposable
    {
        private const int _defaultListeningPort = 8474;

        private readonly string _host;
        private readonly int _port;
        
        private readonly IHttpClientFactory _clientFactory;
        private readonly bool _resetAllToxicsAndProxiesOnClose;
        
        public Connection(bool resetAllToxicsAndProxiesOnClose = false)
            : this("localhost", resetAllToxicsAndProxiesOnClose)
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
            _host = host;
            _port = port;
            _resetAllToxicsAndProxiesOnClose = resetAllToxicsAndProxiesOnClose;
            _clientFactory = new HttpClientFactory(new Uri(string.Format("http://{0}:{1}/", _host, _port)));
        }

        public Client Client()
        {
            return new Client(_clientFactory);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual async Task Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_resetAllToxicsAndProxiesOnClose)
                    {
                        await Client().ResetAsync();
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
            Dispose(true).Wait();
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
        public async ValueTask DisposeAsync()
        {
            await Dispose(true);
        }
    }
}
