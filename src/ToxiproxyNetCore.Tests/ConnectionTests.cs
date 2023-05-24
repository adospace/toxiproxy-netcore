using System;
using System.Threading.Tasks;
using Toxiproxy.Net;
using Xunit;

namespace ToxiproxyNetCore.Tests
{
    [Collection("Integration")]
    public class ConnectionTests : ToxiproxyTestsBase
    {
        public ConnectionTests(ConnectionFixture fixture) : base(fixture) { }
        [Fact]
        public void ErrorThrownIfHostIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new Connection(""));
            Assert.Throws<ArgumentNullException>(() => new Connection(null));
        }

        [Fact]
        public async Task DisposeEnablesAndResetsAllProxies()
        {
            var connection = new Connection(resetAllToxicsAndProxiesOnClose: true);

            var client = connection.Client();
            await client.AddAsync(TestProxy.One);


            var proxy = await client.FindProxyAsync(TestProxy.One.Name);
            proxy.Enabled = false;
            await proxy.UpdateAsync();

            connection.Dispose();

            var proxyCopy = await client.FindProxyAsync(TestProxy.One.Name);
            Assert.True(proxyCopy.Enabled);
        }
    }
}