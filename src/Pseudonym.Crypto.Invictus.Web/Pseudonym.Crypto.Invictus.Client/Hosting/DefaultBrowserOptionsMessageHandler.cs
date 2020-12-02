using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Pseudonym.Crypto.Invictus.Web.Client.Hosting
{
    internal sealed class DefaultBrowserOptionsMessageHandler : DelegatingHandler
    {
        public DefaultBrowserOptionsMessageHandler()
            : base(new HttpClientHandler())
        {
        }

        public DefaultBrowserOptionsMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        public BrowserRequestCache DefaultBrowserRequestCache { get; set; }

        public BrowserRequestCredentials DefaultBrowserRequestCredentials { get; set; }

        public BrowserRequestMode DefaultBrowserRequestMode { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<IDictionary<string, object>>("WebAssemblyFetchOptions"), out IDictionary<string, object> fetchOptions))
            {
                if (fetchOptions?.ContainsKey("cache") != true)
                {
                    request.SetBrowserRequestCache(DefaultBrowserRequestCache);
                }

                if (fetchOptions?.ContainsKey("credentials") != true)
                {
                    request.SetBrowserRequestCredentials(DefaultBrowserRequestCredentials);
                }

                if (fetchOptions?.ContainsKey("mode") != true)
                {
                    request.SetBrowserRequestMode(DefaultBrowserRequestMode);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
