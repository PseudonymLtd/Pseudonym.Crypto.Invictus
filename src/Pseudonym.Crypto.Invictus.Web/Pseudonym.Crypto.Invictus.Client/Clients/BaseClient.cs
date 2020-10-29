using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Clients
{
    internal abstract class BaseClient
    {
        private readonly IHttpClientFactory httpClientFactory;

        public BaseClient(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected virtual async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = await CreateClientAsync();

                client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, Guid.NewGuid().ToString());

                var response = await client.GetAsync(new Uri(url, UriKind.Relative), CancellationToken.None);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TResponse>(json);
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"{GetType().Name} Error calling GET {url}", e);
            }
        }

        protected virtual Task<HttpClient> CreateClientAsync()
        {
            return Task.FromResult(httpClientFactory.CreateClient(GetType().Name));
        }
    }
}
