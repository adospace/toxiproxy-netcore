using System.Collections.Generic;
using System.Threading.Tasks;
using Toxiproxy.Net.Toxics;

namespace Toxiproxy.Net
{
    public class Proxy
    {
        internal Client Client { get; set; }

        public string Name { get; set; }
        public string Listen { get; set; }
        public string Upstream { get; set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// Deletes this proxy.
        /// </summary>
        public Task DeleteAsync() => Client.DeleteAsync(this);

        /// <summary>
        /// Updates this proxy.
        /// </summary>
        /// <returns></returns>
        public Task<Proxy> UpdateAsync() => Client.UpdateAsync(this);

        /// <summary>
        /// Adds the specified toxic to this proxy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toxic">The toxic.</param>
        /// <returns></returns>
        public Task<T> AddAsync<T>(T toxic) where T : ToxicBase => Client.AddToxicToProxyAsync<T>(this, toxic);

        /// <summary>
        /// Gets all the toxics.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<ToxicBase>> GetAllToxicsAsync() => Client.FindAllToxicsByProxyNameAsync(Name);

        /// <summary>
        /// Gets a toxic by name in this proxy.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task<ToxicBase> GetToxicByNameAsync(string name) => Client.FindToxicByProxyNameAndToxicNameAsync(this, name);

        /// <summary>
        /// Removes the toxic.
        /// </summary>
        /// <param name="toxicName">Name of the toxic.</param>
        public Task RemoveToxicAsync(string toxicName) => Client.RemoveToxicFromProxyAsync(Name, toxicName);

        /// <summary>
        /// Updates the toxic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toxicName">Name of the toxic.</param>
        /// <param name="toxic">The toxic.</param>
        public Task UpdateToxicAsync<T>(string toxicName, T toxic) where T : ToxicBase => Client.UpdateToxicAsync(Name, toxicName, toxic);
    }
}