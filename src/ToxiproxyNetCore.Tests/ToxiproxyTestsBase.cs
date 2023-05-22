using System;
using System.Threading.Tasks;
using Xunit;

namespace Toxiproxy.Net.Tests
{
    public class ToxiproxyTestsBase : IDisposable, IAsyncLifetime
    {
        protected Connection _connection;

        protected readonly Proxy ProxyOne = new Proxy
        {
            Name = "one",
            Enabled = true,
            Listen = "127.0.0.1:11111",
            Upstream = "one.com"
        };

        protected readonly Proxy ProxyTwo = new Proxy {
            Name = "two",
            Enabled = true,
            Listen = "127.0.0.1:22222",
            Upstream = "two.com"
        };

        protected readonly Proxy ProxyThree = new Proxy {
            Name = "three",
            Enabled = true,
            Listen = "127.0.0.1:33333",
            Upstream = "three.com"
        };

        public ToxiproxyTestsBase() 
        {
            _connection = new Connection(resetAllToxicsAndProxiesOnClose: true);
            
            _connection.Client().ResetAsync().Wait();
            var proxies = _connection.Client().AllAsync().GetAwaiter().GetResult();
            foreach (var proxy in proxies)
            {
                proxy.Value.DeleteAsync().GetAwaiter().GetResult();
            }
            
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public async Task InitializeAsync()
        {
            _connection = new Connection(resetAllToxicsAndProxiesOnClose: true);

            await _connection.Client().ResetAsync();
            var proxies = await _connection.Client().AllAsync();
            foreach (var proxy in proxies)
            {
                proxy.Value.DeleteAsync().GetAwaiter().GetResult();
            }
        }

        public Task DisposeAsync()
        {
            _connection?.Dispose();
            return Task.CompletedTask;
        }
    }
}