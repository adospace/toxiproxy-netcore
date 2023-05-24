using System.Diagnostics;
using System.Linq;
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
        

        public async Task InitializeAsync()
        {
            /*_process = new Process
           {
               StartInfo = new ProcessStartInfo { FileName = @"./toxiproxy-server.exe" }
           };

           _process.Start();*/
            await Task.Delay(500);
        }

        public async Task ResetConnection()
        {
            _connection = new Connection(resetAllToxicsAndProxiesOnClose: true);

            await _connection.Client().ResetAsync();
            var proxies = await _connection.Client().AllAsync();
            await Task.WhenAll(proxies.Select(proxy => proxy.Value.DeleteAsync()));
        }

        public async Task DisposeAsync() => await Task.Run(Dispose);
        

        public void DisposeConnection() => _connection?.Dispose();


        public void Dispose()
        {
            DisposeConnection();
            _process?.Kill();
        }
    }
}
