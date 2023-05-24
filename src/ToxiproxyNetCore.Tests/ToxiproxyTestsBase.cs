using Xunit;

namespace ToxiproxyNetCore.Tests
{
    public class ToxiproxyTestsBase : IClassFixture<ConnectionFixture>
    {
        protected readonly ConnectionFixture Fixture;

        public ToxiproxyTestsBase(ConnectionFixture connectionFixture)
        {
            Fixture = connectionFixture;
        }
    }
}