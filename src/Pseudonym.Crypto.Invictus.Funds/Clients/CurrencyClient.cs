using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class CurrencyClient : ICurrencyClient
    {
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public CurrencyClient(
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<CurrencyRates> GetRatesAsync()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(nameof(CurrencyClient));

                var response = await client.GetAsync(new Uri("/v6/latest", UriKind.Relative), scopedCancellationToken.Token);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<CurrencyRates>(json);
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"{GetType().Name} Error calling GET /v6/latest", e);
            }
        }
    }
}
