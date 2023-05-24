using System.Reflection;

namespace ToxiproxyNetCore.Tests.Resources
{
    public static class ResourceLoader
    {
        public static byte[] LoadResourceAsByteArray(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(string.Concat("ToxiproxyNetCore.Tests.Resources.", resourceName));
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
