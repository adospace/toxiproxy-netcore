using System;
using System.Diagnostics;
using System.IO;
using ToxiproxyNetCore.Tests.Resources;

namespace Toxiproxy.Net.Tests
{
    public class ToxiproxyTestsBase : IDisposable
    {
        protected Connection _connection;
        protected Process _process;

        protected readonly Proxy ProxyOne = new Proxy
        {
            Name = "one",
            Enabled = true,
            Listen = "127.0.0.1:11111",
            Upstream = "one.com"
        };

        protected readonly Proxy ProxyTwo = new Proxy {
            Name = "two",
            Enabled = true,
            Listen = "127.0.0.1:22222",
            Upstream = "two.com"
        };

        protected readonly Proxy ProxyThree = new Proxy {
            Name = "three",
            Enabled = true,
            Listen = "127.0.0.1:33333",
            Upstream = "three.com"
        };

        private bool _firstRun = false;

        public ToxiproxyTestsBase() 
        {
            var testFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var targetServerPath = Path.Combine(testFolder, "toxiproxy-server-windows-amd64.exe");

            if (!File.Exists(targetServerPath) || _firstRun)
            {
                File.WriteAllBytes(targetServerPath, ResourceLoader.LoadResourceAsByteArray("toxiproxy-server-windows-amd64.exe"));
            }

            var processInfo = new ProcessStartInfo()
            {
                FileName = targetServerPath
            };
            _process = new Process()
            {
                StartInfo = processInfo
            };
            _process.Start();
            
            _connection = new Connection();
        }

        public void Dispose()
        {
            if (_process.HasExited == false)
            {
                _process.Kill();
            }
        }
    }
}