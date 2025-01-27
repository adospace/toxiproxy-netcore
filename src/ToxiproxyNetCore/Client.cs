using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toxiproxy.Net.Toxics;

namespace Toxiproxy.Net
{
    /// <summary>
    /// The client to send the requests to the ToxiProxy server
    /// </summary>
    public class Client
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonConverter[] _deserializeConverter = { new JsonToxicsConverter() };
        private readonly Lazy<Task<string>> _serverVersion;

        public Client(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _serverVersion = new Lazy<Task<string>>(GetServerVersionAsync);
        }

        public async Task<IDictionary<string, Proxy>> AllAsync()
        {
            using (var httpClient = _clientFactory.Create())
            {
                var response = await httpClient.GetAsync("/proxies");
                await CheckIsSuccessStatusCode(response);

                var responseResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, Proxy>>(responseResult);

                foreach (var proxy in result.Values)
                {
                    proxy.Client = this;
                }

                return result;
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public async Task ResetAsync()
        {
            using (var httpClient = _clientFactory.Create())
            {
                var response = await httpClient.PostAsync("/reset", null);

                await CheckIsSuccessStatusCode(response);
            }
        }

        #region Proxy
        /// <summary>
        /// Adds the specified proxy to the ToxiProxy server.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">proxy</exception>
        public async Task<Proxy> AddAsync(Proxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            using (var httpClient = _clientFactory.Create())
            {
                var postPayload = JsonConvert.SerializeObject(proxy);
                var response = await httpClient.PostAsync("/proxies", new StringContent(postPayload, Encoding.UTF8, "application/json"));
                await CheckIsSuccessStatusCode(response);

                var responseResult = await response.Content.ReadAsStringAsync();
                JsonConvert.PopulateObject(responseResult, proxy);

                proxy.Client = this;

                return proxy;
            }
        }

        /// <summary>
        /// Updates the specified proxy.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">proxy</exception>
        public async Task<Proxy> UpdateAsync(Proxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            using (var httpClient = _clientFactory.Create())
            {
                var url = $"/proxies/{proxy.Name}";
                var postPayload = JsonConvert.SerializeObject(proxy);
                HttpResponseMessage response;
                if (await ServerSupportsHttpPatchForProxyUpdates())
                {
                    response = await httpClient.PatchAsync(url, new StringContent(postPayload, Encoding.UTF8, "application/json"));
                }
                else
                {
                    response = await httpClient.PostAsync(url, new StringContent(postPayload, Encoding.UTF8, "application/json"));
                }

                await CheckIsSuccessStatusCode(response);

                var responseResult = await response.Content.ReadAsStringAsync();
                JsonConvert.PopulateObject(responseResult, proxy);

                proxy.Client = this;

                return proxy;
            }
        }

        /// <summary>
        /// Finds the proxy by name.
        /// </summary>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">proxyName</exception>
        public async Task<Proxy> FindProxyAsync(string proxyName)
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                throw new ArgumentNullException(nameof(proxyName));
            }

            using (var httpClient = _clientFactory.Create())
            {
                var url = $"/proxies/{proxyName}";
                var response = await httpClient.GetAsync(url);

                await CheckIsSuccessStatusCode(response);

                var resultString = await response.Content.ReadAsStringAsync();
                var proxy = JsonConvert.DeserializeObject<Proxy>(resultString);

                proxy.Client = this;

                return proxy;
            }
        }

        /// <summary>
        /// Deletes the specified proxy.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <exception cref="ArgumentNullException">proxy</exception>
        public Task DeleteAsync(Proxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }
            return DeleteAsync(proxy.Name);
        }

        /// <summary>
        /// Deletes the specified proxy name.
        /// </summary>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <exception cref="ArgumentNullException">proxyName</exception>
        public async Task DeleteAsync(string proxyName)
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                throw new ArgumentNullException(nameof(proxyName));
            }

            using (var httpClient = _clientFactory.Create())
            {
                var response = await httpClient.DeleteAsync($"/proxies/{proxyName}");
                await CheckIsSuccessStatusCode(response);
            }
        }
        #endregion

        #region Toxic
        /// <summary>
        /// Finds a toxic by proxy name and toxic name.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <param name="toxicName">Name of the toxic.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// proxy
        /// or
        /// toxic name
        /// </exception>
        internal async Task<ToxicBase> FindToxicByProxyNameAndToxicNameAsync(Proxy proxy, string toxicName)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            if (string.IsNullOrEmpty(toxicName))
            {
                throw new ArgumentNullException(nameof(toxicName));
            }

            using (var httpClient = _clientFactory.Create())
            {
                var response = await httpClient.GetAsync($"/proxies/{proxy.Name}/toxics/{toxicName}");
                await CheckIsSuccessStatusCode(response);

                var responseContent = await response.Content.ReadAsStringAsync();
                var toxic = JsonConvert.DeserializeObject<ToxicBase>(responseContent, _deserializeConverter);

                toxic.Client = this;
                toxic.ParentProxy = proxy;

                return toxic;
            }
        }

        /// <summary>
        /// Adds the toxic to the specific proxy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxy">The proxy.</param>
        /// <param name="toxic">The toxic.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// proxy
        /// or
        /// toxic
        /// </exception>
        internal async Task<T> AddToxicToProxyAsync<T>(Proxy proxy, T toxic) where T : ToxicBase
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            if (toxic == null)
            {
                throw new ArgumentNullException(nameof(toxic));
            }

            using (var client = _clientFactory.Create())
            {
                var url = $"proxies/{proxy.Name}/toxics";
                var objectSerialized = JsonConvert.SerializeObject( 
                        toxic, 
                        new JsonSerializerSettings {
                            NullValueHandling = NullValueHandling.Ignore
                        } 
                    );

                var response = await client.PostAsync(url, new StringContent(objectSerialized, Encoding.UTF8, "application/json"));

                await CheckIsSuccessStatusCode(response);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(content);

                toxic.Client = this;
                toxic.ParentProxy = proxy;

                return result;
            }
        }

        /// <summary>
        /// Finds the name of the toxics in the proxy with the specified name.
        /// </summary>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <returns></returns>
        internal async Task<IEnumerable<ToxicBase>> FindAllToxicsByProxyNameAsync(string proxyName)
        {
            using (var client = _clientFactory.Create())
            {
                var url = $"proxies/{proxyName}/toxics";
                var response = await client.GetAsync(url);

                await CheckIsSuccessStatusCode(response);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<ToxicBase>>(content, _deserializeConverter);
                return result;
            }
        }

        /// <summary>
        /// Removes the toxic from a proxy.
        /// </summary>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <param name="toxicName">Name of the toxic.</param>
        internal async Task RemoveToxicFromProxyAsync(string proxyName, string toxicName)
        {
            using (var client = _clientFactory.Create())
            {
                var url = $"/proxies/{proxyName}/toxics/{toxicName}";
                var response = await client.DeleteAsync(url);

                await CheckIsSuccessStatusCode(response);
            }
        }

        /// <summary>
        /// Updates the toxic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <param name="existingToxicName">Name of the existing toxic.</param>
        /// <param name="toxic">The toxic.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// proxyName
        /// or
        /// toxic
        /// </exception>
        internal async Task<T> UpdateToxicAsync<T>(string proxyName, string existingToxicName, T toxic) where T : ToxicBase
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                throw new ArgumentNullException(nameof(proxyName));
            }

            if (toxic == null)
            {
                throw new ArgumentNullException(nameof(toxic));
            }
            
            using (var client = _clientFactory.Create())
            {
                var url = $"/proxies/{proxyName}/toxics/{existingToxicName}";
                var objectSerialized = JsonConvert.SerializeObject(toxic);
                HttpResponseMessage response;
                if (await ServerSupportsHttpPatchForProxyUpdates())
                {
                    response = await client.PatchAsync(url, new StringContent(objectSerialized, Encoding.UTF8, "application/json"));
                }
                else
                {
                    response = await client.PostAsync(url, new StringContent(objectSerialized, Encoding.UTF8, "application/json"));
                }

                await CheckIsSuccessStatusCode(response);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(content, _deserializeConverter);
                return result;
            }
        }

        /// <summary>
        /// Checks the response status code and throw exceptions in case of failure status code.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <exception cref="ToxiProxiException">
        /// Not found
        /// or
        /// duplicated entity
        /// or
        /// An error occurred: " + error.title
        /// </exception>
        #endregion

        private async Task CheckIsSuccessStatusCode(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound: 
                    throw new ToxiProxiException("Not found");

                case HttpStatusCode.Conflict: 
                    throw new ToxiProxiException("duplicated entity");

                default: 
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ToxiProxiErrorMessage>(errorContent);
                    throw new ToxiProxiException("An error occurred: " + error.title);
            }
        }

        /// <summary>
        /// Get the Toxiproxy server version.
        /// </summary>
        /// <returns>The Toxiproxy server version.</returns>
        public Task<string> VersionAsync()
        {
            return _serverVersion.Value;
        }

        /// <summary>
        /// Get the Toxiproxy server version calling the dedicated endpoint.
        /// </summary>
        /// <returns>The server version as <see cref="Version"/> object.</returns>
        private async Task<string> GetServerVersionAsync()
        {
            using (var httpClient = _clientFactory.Create())
            {
                var response = await httpClient.GetAsync("/version");
                await CheckIsSuccessStatusCode(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Starting from version 2.6.0, Toxiproxy server supports HTTP PATCH method for proxy updates, 
        /// and started deprecating updates using HTTP POST.
        /// This method helps checking the server version so to allow using the preferred update HTTP method 
        /// according to the server version, so we can support both ways without breaking.
        /// <see href="https://github.com/Shopify/toxiproxy/blob/main/CHANGELOG.md#260---2023-08-22">See Toxiproxy changelog.</see>
        /// </summary>
        /// <returns><see cref="true"/> if server version is 2.6.0 or above.</returns>
        private async Task<bool> ServerSupportsHttpPatchForProxyUpdates()
        {
            Version supportsPatchMethodForUpdates = new Version("2.6.0");

            var serverVersion = new Version(await _serverVersion.Value);
            return !(serverVersion.CompareTo(supportsPatchMethodForUpdates) < 0);
        }
    }
}