using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Hosting
{
    internal sealed class SessionStore : ISessionStore, IAsyncDisposable
    {
        private readonly IJSRuntime jsRuntime;

        public SessionStore(IJSRuntime jSRuntime)
        {
            jsRuntime = jSRuntime;
        }

        public async Task SetAsync<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data);

            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, serialisedData);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);

            if (serialisedData == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(serialisedData);
        }

        public async Task RemoveItemAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            await jsRuntime.InvokeAsync<object>("sessionStorage.removeItem", key);
        }

        public async Task ClearAsync() => await jsRuntime.InvokeAsync<object>("sessionStorage.clear");

        public async Task<int> LengthAsync() => await jsRuntime.InvokeAsync<int>("eval", "sessionStorage.length");

        public async Task<string> KeyAsync(int index) => await jsRuntime.InvokeAsync<string>("sessionStorage.key", index);

        public async ValueTask DisposeAsync()
        {
            await ClearAsync();
        }
    }
}
