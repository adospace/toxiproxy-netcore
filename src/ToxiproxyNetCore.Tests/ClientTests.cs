using System;
using System.Linq;
using System.Threading.Tasks;
using Toxiproxy.Net.Toxics;
using Xunit;

namespace Toxiproxy.Net.Tests
{
    [Collection("Integration")]
    public class ClientTests : ToxiproxyTestsBase
    {
        [Fact]
        public async Task ErrorsAreThrownCorrectly()
        {
            var client = _connection.Client();

            await Assert.ThrowsAsync<ToxiProxiException>(async () =>
	            await client.FindProxyAsync("DOESNOTEXIST"));
        }

        [Fact]
        public async Task CanFindNamedProxy()
        {
            // Create a proxy and add the proxy to the client
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);

            // Retrieve the proxy
            var proxy = await client.FindProxyAsync("one");
            
            // Check if it the correct one
            Assert.NotNull(proxy);
            Assert.Equal(proxy.Name, ProxyOne.Name);
            Assert.Equal(proxy.Upstream, ProxyOne.Upstream);
        }

        [Fact]
        public async Task CanFindAllProxies()
        {
            // Create two proxies and add them to the client
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);
            await client.AddAsync(ProxyTwo);

            // Retrieve all the proxies
            var all = await client.AllAsync();

            // Check if there are two proxies
            Assert.Equal(2, all.Keys.Count);
            // Check if contains the correct proxies
            Assert.True(all.ContainsKey(ProxyOne.Name));
            Assert.True(all.ContainsKey(ProxyTwo.Name));
        }

        [Fact]
        public async Task CanDeleteProxy()
        {
            // Add three proxies
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);
            await client.AddAsync(ProxyTwo);
            await client.AddAsync(ProxyThree);

            // Delete two proxies
            await client.DeleteAsync(ProxyOne);
            await client.DeleteAsync(ProxyTwo.Name);

            // The client should contain only a proxy
            var all = await client.AllAsync();
            Assert.Equal(1, all.Keys.Count);

            // The single proxy in the collection should be the 3th proxy
            var containsProxyThree = all.ContainsKey(ProxyThree.Name);
            Assert.True(containsProxyThree);
        }

        [Fact]
        public async Task CanUpdateProxy()
        {
            // Add a proxy
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);

            // Retrieve the proxy and update the proxy
            var proxyToUpdate = await client.FindProxyAsync(ProxyOne.Name);
            proxyToUpdate.Enabled = false;
            proxyToUpdate.Listen = "127.0.0.1:55555";
            proxyToUpdate.Upstream = "google.com";
            await client.UpdateAsync(proxyToUpdate);

            // Retrieve the proxy and check if the parameters are correctly updated
            var proxyUpdated = await client.FindProxyAsync(proxyToUpdate.Name);

            Assert.Equal(proxyToUpdate.Enabled, proxyUpdated.Enabled);
            Assert.Equal(proxyToUpdate.Listen, proxyUpdated.Listen);
            Assert.Equal(proxyToUpdate.Upstream, proxyUpdated.Upstream);
        }

        [Fact]
        public async Task ResetWorks()
        {
            // Add a disabled proxy
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);

            // Reset
            await client.ResetAsync();

            // Retrieve the proxy
            var proxyCopy = await client.FindProxyAsync(ProxyOne.Name);

            // The proxy should be enabled
            Assert.True(proxyCopy.Enabled);
        }

        [Fact]
        public async Task CanNotAddANullProxy()
        {
            var client = _connection.Client();

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.AddAsync(null));
        }

        [Fact]
        public async Task CreateANewProxyShouldWork()
        {
            var client = _connection.Client();
            var newProxy = await client.AddAsync(ProxyOne);

            Assert.Equal(ProxyOne.Name, newProxy.Name);
            Assert.Equal(ProxyOne.Enabled, newProxy.Enabled);
            Assert.Equal(ProxyOne.Listen, newProxy.Listen);
            Assert.Equal(ProxyOne.Upstream, newProxy.Upstream);
        }

        [Fact]
        public async Task DeletingAProxyMoreThanOnceShouldThrowException()
        {
            // Add a proxy and check it exists
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);
            var proxy = await client.FindProxyAsync(ProxyOne.Name);    

            // deleting is not idemnepotent and should throw exception
            await proxy.DeleteAsync();
            var exception = await Assert.ThrowsAsync<ToxiProxiException>(async () => await proxy.DeleteAsync());
            Assert.Equal("Not found", exception.Message);
        }

        [Fact]
        public async Task DeletingAProxyWorks()
        {
            // Add a proxy and check it exists
            var client = _connection.Client();
            await client.AddAsync(ProxyOne);
            var proxy = await client.FindProxyAsync(ProxyOne.Name);

            // delete
            await proxy.DeleteAsync();

            // check it doesn't exists
            await Assert.ThrowsAsync<ToxiProxiException>(async () =>
                await client.FindProxyAsync(ProxyOne.Name));
        }

		[Fact]
		public async Task AddToxic_NullFields() {
			// Create a proxy and add the proxy to the client
			var client = _connection.Client();
			await client.AddAsync( ProxyOne );

			// Retrieve the proxy
			var proxy = await client.FindProxyAsync("one");
			var latencyToxic = new LatencyToxic();
			latencyToxic.Attributes.Latency = 1000;

			await proxy.AddAsync( latencyToxic );
			await proxy.UpdateAsync();

			var toxics = await proxy.GetAllToxicsAsync();
			Assert.True(toxics.Count() == 1);
			var toxic = toxics.First();

			Assert.Equal( 1, toxic.Toxicity );
			Assert.Equal( ToxicDirection.DownStream, toxic.Stream );

			//default pattern is <type>_<stream>
			Assert.Equal( "latency_downstream", toxic.Name );
		}

		[Fact]
		public async Task AddToxic_NonNullFields() {
			// Create a proxy and add the proxy to the client
			var client = _connection.Client();
			await client.AddAsync( ProxyOne );

			// Retrieve the proxy
			var proxy = await client.FindProxyAsync( "one" );

			var latencyToxic = new LatencyToxic();
			latencyToxic.Attributes.Latency = 1000;
			latencyToxic.Stream = ToxicDirection.UpStream;
			latencyToxic.Name = "testName";
			latencyToxic.Toxicity = 0.5;

			await proxy.AddAsync( latencyToxic );
			await proxy.UpdateAsync();

			var toxics = await proxy.GetAllToxicsAsync();
			Assert.True(toxics.Count() == 1);
			var toxic = toxics.First();

			Assert.Equal( 0.5, toxic.Toxicity );
			Assert.Equal( ToxicDirection.UpStream, toxic.Stream );

			//default pattern is <type>_<stream>
			Assert.Equal( "testName", toxic.Name );
		}
	}
}
