using System.Threading.Tasks;
using Xunit;

namespace ToxiproxyNetCore.Tests
{
    public class ToxiproxyTestsBase : IClassFixture<ConnectionFixture>, IAsyncLifetime
    {
        protected readonly ConnectionFixture Fixture;

        public ToxiproxyTestsBase(ConnectionFixture connectionFixture)
        {
            Fixture = connectionFixture;
        }

        public async Task InitializeAsync() => await Fixture.ResetConnection();
        

        public async Task DisposeAsync() => await Task.Run(Fixture.DisposeConnection);
    }
}