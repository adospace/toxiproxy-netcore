using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Toxiproxy.Net
{
    internal static class HttpClientExtensions
    {
        private static readonly HttpMethod PatchHttpMethod = new HttpMethod("PATCH");

        public static Task<HttpResponseMessage> PatchAsync(this HttpClient httpClient, string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return httpClient.PatchAsync(CreateUri(requestUri), content, cancellationToken);
        }

        private static Task<HttpResponseMessage> PatchAsync(this HttpClient httpClient, Uri requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = new HttpRequestMessage(PatchHttpMethod, requestUri)
            {
                Content = content
            };
            return httpClient.SendAsync(request, cancellationToken);
        }

        private static Uri CreateUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return null;
            }
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
}
