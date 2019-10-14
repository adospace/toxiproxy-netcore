using System;
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
        public void DisposeEnablesAndResetsAllProxies()
        {
            var connection = new Connection(resetAllToxicsAndProxiesOnClose: true);

            var client = connection.Client();
            client.AddAsync(ProxyOne).Wait();

            var proxy = client.FindProxyAsync(ProxyOne.Name).Result;
            proxy.Enabled = false;
            proxy.UpdateAsync().Wait();

            connection.Dispose();

            var proxyCopy = client.FindProxyAsync(ProxyOne.Name).Result;
            Assert.True(proxyCopy.Enabled);
        }
    }
}