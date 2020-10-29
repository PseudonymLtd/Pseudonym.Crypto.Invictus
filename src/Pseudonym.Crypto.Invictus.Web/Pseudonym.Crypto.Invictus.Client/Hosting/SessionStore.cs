using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Hosting
{
    internal sealed class SessionStore : ISessionStore, IAsyncDisposable
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
                new VersionConverter(),
                new IsoDateTimeConverter()
            }
        };

        private readonly IJSInProcessRuntime jsRuntime;

        public SessionStore(IJSRuntime jSRuntime)
        {
            jsRuntime = (IJSInProcessRuntime)jSRuntime;
        }

        public void Set<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data, SerializerSettings);

            jsRuntime.InvokeVoid("sessionStorage.setItem", key, serialisedData);
        }

        public async Task SetAsync<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data, SerializerSettings);

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

            return JsonConvert.DeserializeObject<T>(serialisedData, SerializerSettings);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = jsRuntime.Invoke<string>("sessionStorage.getItem", key);

            if (serialisedData == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(serialisedData, SerializerSettings);
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
