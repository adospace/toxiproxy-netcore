using Toxiproxy.Net;

namespace ToxiproxyNetCore.Tests
{
    internal static class TestProxy
    {
        public static readonly Proxy[] TestProxies = new Proxy[]
        {
            new()
            {
                Name = "one",
                Enabled = true,
                Listen = "127.0.0.1:11111",
                Upstream = "one.com"
            },
            new()
            {
                Name = "two",
                Enabled = true,
                Listen = "127.0.0.1:22222",
                Upstream = "two.com"
            },
            new()
            {
                Name = "three",
                Enabled = true,
                Listen = "127.0.0.1:33333",
                Upstream = "three.com"
            }
        };
        public static Proxy One => TestProxies[0];
        public static Proxy Two => TestProxies[1];
        public static Proxy Three => TestProxies[2];
    }
}
