using System;
using System.Net.Http;

namespace Toxiproxy.Net
{
    /// <summary>
    /// The factory class to create preconfigured HttpClient
    /// </summary>
    /// <seealso cref="Toxiproxy.Net.IHttpClientFactory" />
    internal class HttpClientFactory : IHttpClientFactory
    {

        public HttpClientFactory(Uri baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public Uri BaseUrl { get; }

    public HttpClient Create()
        {
            var client = new HttpClient { BaseAddress = BaseUrl };
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            return client;
        }
    }

    public interface IHttpClientFactory
    {
        Uri BaseUrl { get; }
        HttpClient Create();
    }
}
