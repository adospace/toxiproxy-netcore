using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toxiproxy.Net;
using Xunit;

namespace ToxiproxyNetCore.Tests
{
    public class ConnectionFixture: IAsyncLifetime
    {
        private readonly Process _process;
        private Connection _connection;
        public Client Client => _connection.Client();
        

        public ConnectionFixture()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo { FileName = @"./toxiproxy-server.exe" }
            };

            _process.Start();
            Thread.Sleep(500);
            _connection = new Connection(resetAllToxicsAndProxiesOnClose: true);
        }

        public async Task InitializeAsync()
        {
            await ResetConnection();
        }

        public async Task ResetConnection()
        {
            _connection = new Connection(resetAllToxicsAndProxiesOnClose: true);

            await _connection.Client().ResetAsync();
            var proxies = await _connection.Client().AllAsync();
            await Task.WhenAll(proxies.Select(proxy => proxy.Value.DeleteAsync()));
        }

        public async Task DisposeAsync()
        {
            Dispose();
        }


        public void Dispose()
        {
            _connection.Dispose();
            _process?.Kill();
        }
    }
}
