using System;
using System.Linq;
using System.Threading.Tasks;
using Toxiproxy.Net;
using Toxiproxy.Net.Toxics;
using Xunit;

namespace ToxiproxyNetCore.Tests
{
    [Collection("Integration")]
    public class ClientTests : ToxiproxyTestsBase
    {
        public ClientTests(ConnectionFixture fixture) : base(fixture) { }

        [Fact]
        public async Task ErrorsAreThrownCorrectly()
        {
            await Assert.ThrowsAsync<ToxiProxiException>(async () =>
                await Fixture.Client.FindProxyAsync("DOESNOTEXIST"));
        }

        [Fact]
        public async Task CanFindNamedProxy()
        {
            // Create a proxy and add the proxy to the client
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);

            // Retrieve the proxy
            var proxy = await client.FindProxyAsync("one");
            // Check if it the correct one
            Assert.NotNull(proxy);
            Assert.Equal(proxy.Name, TestProxy.One.Name);
            Assert.Equal(proxy.Upstream, TestProxy.One.Upstream);
        }

        [Fact]
        public async Task CanFindAllProxies()
        {
            // Create two proxies and add them to the client
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);
            await client.AddAsync(TestProxy.Two);

            // Retrieve all the proxies
            var all = await client.AllAsync();

            // Check if there are two proxies
            Assert.Equal(2, all.Keys.Count);
            // Check if contains the correct proxies
            Assert.True(all.ContainsKey(TestProxy.One.Name));
            Assert.True(all.ContainsKey(TestProxy.Two.Name));
        }

        [Fact]
        public async Task CanDeleteProxy()
        {
            // Add three proxies
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);
            await client.AddAsync(TestProxy.Two);
            await client.AddAsync(TestProxy.Three);

            // Delete two proxies
            await client.DeleteAsync(TestProxy.One);
            await client.DeleteAsync(TestProxy.Two.Name);

            // The client should contain only a proxy
            var all = await client.AllAsync();
            Assert.Equal(1, all.Keys.Count);

            // The single proxy in the collection should be the 3th proxy
            var containsProxyThree = all.ContainsKey(TestProxy.Three.Name);
            Assert.True(containsProxyThree);
        }

        [Fact]
        public async Task CanUpdateProxy()
        {
            // Add a proxy
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);

            // Retrieve the proxy and update the proxy
            var proxyToUpdate = await client.FindProxyAsync(TestProxy.One.Name);
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
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);

            // Reset
            await client.ResetAsync();

            // Retrieve the proxy
            var proxyCopy = await client.FindProxyAsync(TestProxy.One.Name);
            
            // The proxy should be enabled
            Assert.True(proxyCopy.Enabled);
        }

        [Fact]
        public async Task CanNotAddANullProxy()
        {
            var client = Fixture.Client;

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.AddAsync(null));
        }

        [Fact]
        public async Task CreateANewProxyShouldWork()
        {
            var client = Fixture.Client;
            var newProxy = await client.AddAsync(TestProxy.One);

            Assert.Equal(TestProxy.One.Name, newProxy.Name);
            Assert.Equal(TestProxy.One.Enabled, newProxy.Enabled);
            Assert.Equal(TestProxy.One.Listen, newProxy.Listen);
            Assert.Equal(TestProxy.One.Upstream, newProxy.Upstream);
        }

        [Fact]
        public async Task DeletingAProxyMoreThanOnceShouldThrowException()
        {
            // Add a proxy and check it exists
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);
            var proxy = await client.FindProxyAsync(TestProxy.One.Name);

            // deleting is not idemnepotent and should throw exception
            await proxy.DeleteAsync();
            var exception = await Assert.ThrowsAsync<ToxiProxiException>(async () => await proxy.DeleteAsync());
            Assert.Equal("Not found", exception.Message);
        }

        [Fact]
        public async Task DeletingAProxyWorks()
        {
            // Add a proxy and check it exists
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);
            var proxy = await client.FindProxyAsync(TestProxy.One.Name);

            // delete
            await proxy.DeleteAsync();

            // check it doesn't exists
            await Assert.ThrowsAsync<ToxiProxiException>(async () =>
                await client.FindProxyAsync(TestProxy.One.Name));
        }

        [Fact]
        public async Task AddToxic_NullFields()
        {
            // Create a proxy and add the proxy to the client
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);

            // Retrieve the proxy
            var proxy = await client.FindProxyAsync("one");
            var latencyToxic = new LatencyToxic();
            latencyToxic.Attributes.Latency = 1000;

            await proxy.AddAsync(latencyToxic);
            await proxy.UpdateAsync();

            var toxics = await proxy.GetAllToxicsAsync();
            Assert.True(toxics.Count() == 1);
            var toxic = toxics.First();

            Assert.Equal(1, toxic.Toxicity);
            Assert.Equal(ToxicDirection.DownStream, toxic.Stream);

            //default pattern is <type>_<stream>
            Assert.Equal("latency_downstream", toxic.Name);
        }

        [Fact]
        public async Task AddToxic_NonNullFields()
        {
            // Create a proxy and add the proxy to the client
            var client = Fixture.Client;
            await client.AddAsync(TestProxy.One);

            // Retrieve the proxy
            var proxy = await client.FindProxyAsync("one");

            var latencyToxic = new LatencyToxic();
            latencyToxic.Attributes.Latency = 1000;
            latencyToxic.Stream = ToxicDirection.UpStream;
            latencyToxic.Name = "testName";
            latencyToxic.Toxicity = 0.5;

            await proxy.AddAsync(latencyToxic);
            await proxy.UpdateAsync();

            var toxics = await proxy.GetAllToxicsAsync();
            Assert.True(toxics.Count() == 1);
            var toxic = toxics.First();

            Assert.Equal(0.5, toxic.Toxicity);
            Assert.Equal(ToxicDirection.UpStream, toxic.Stream);

            //default pattern is <type>_<stream>
            Assert.Equal("testName", toxic.Name);
        }
    }
}
