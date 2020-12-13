using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Utils.Interop;

namespace Pseudonym.Crypto.Invictus.Web.Client.Hosting
{
    internal sealed class CookieManager : JService, ICookieManager
    {
        public CookieManager(IJSRuntime jsRuntime)
            : base(jsRuntime)
        {
        }

        public void Set<T>(string key, T data, int days)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data, SerializerSettings);

            Runtime.Invoke<object>("functions.WriteCookie", key, serialisedData, days);
        }

        public async Task SetAsync<T>(string key, T data, int days)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = JsonConvert.SerializeObject(data, SerializerSettings);

            await Runtime.InvokeAsync<object>("functions.WriteCookie", key, serialisedData, days);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var serialisedData = Runtime.Invoke<string>("functions.ReadCookie", key);
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

            var serialisedData = await Runtime.InvokeAsync<string>("functions.ReadCookie", key);
            if (serialisedData == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(serialisedData, SerializerSettings);
        }
    }
}
