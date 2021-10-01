using System;
using System.Threading.Tasks;
using Xunit;

namespace Toxiproxy.Net.Tests
{
    [Collection("Integration")]
    public class ConnectionTests : ToxiproxyTestsBase
    {
        [Fact]
        public void ErrorThrownIfHostIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var connection = new Connection("");

            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                var connection = new Connection(null);
            });
        }

        [Fact]
        public async Task DisposeEnablesAndResetsAllProxies()
        {
            var connection = new Connection(resetAllToxicsAndProxiesOnClose: true);

            var client = connection.Client();
            await client.AddAsync(ProxyOne);

            var proxy = await client.FindProxyAsync(ProxyOne.Name);
            proxy.Enabled = false;
            await proxy.UpdateAsync();

            connection.Dispose();

            var proxyCopy = await client.FindProxyAsync(ProxyOne.Name);
            Assert.True(proxyCopy.Enabled);
        }
    }
}