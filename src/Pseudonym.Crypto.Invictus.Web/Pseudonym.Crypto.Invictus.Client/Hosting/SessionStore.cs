﻿using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Utils.Interop;

namespace Pseudonym.Crypto.Invictus.Web.Client.Hosting
{
    internal sealed class SessionStore : JService, ISessionStore, IAsyncDisposable
    {
        public SessionStore(IJSRuntime jsRuntime)
            : base(jsRuntime)
        {
        }

        public void Set<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data, SerializerSettings);

            Runtime.InvokeVoid("sessionStorage.setItem", key, serialisedData);
        }

        public async Task SetAsync<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data, SerializerSettings);

            await Runtime.InvokeVoidAsync("sessionStorage.setItem", key, serialisedData);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = Runtime.Invoke<string>("sessionStorage.getItem", key);
            if (serialisedData == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(serialisedData, SerializerSettings);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = await Runtime.InvokeAsync<string>("sessionStorage.getItem", key);
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

            await Runtime.InvokeAsync<object>("sessionStorage.removeItem", key);
        }

        public async Task ClearAsync() => await Runtime.InvokeAsync<object>("sessionStorage.clear");

        public async Task<int> LengthAsync() => await Runtime.InvokeAsync<int>("eval", "sessionStorage.length");

        public async Task<string> KeyAsync(int index) => await Runtime.InvokeAsync<string>("sessionStorage.key", index);

        public async ValueTask DisposeAsync()
        {
            await ClearAsync();
        }
    }
}
