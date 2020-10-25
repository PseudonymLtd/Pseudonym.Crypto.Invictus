using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;
using Pseudonym.Crypto.Invictus.Funds.Hosting.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class CurrencyClient : ICurrencyClient
    {
        private readonly IScopedCorrelation scopedCorrelation;
        private readonly IHttpClientFactory httpClientFactory;

        public CurrencyClient(
            IScopedCorrelation scopedCorrelation,
            IHttpClientFactory httpClientFactory)
        {
            this.scopedCorrelation = scopedCorrelation;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<CurrencyRates> GetRatesAsync(CancellationToken cancellationToken)
        {
            using var client = httpClientFactory.CreateClient(nameof(CurrencyClient));

            client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, scopedCorrelation.CorrelationId);

            var response = await client.GetAsync(new Uri("/v6/latest", UriKind.Relative), cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CurrencyRates>(json);
        }
    }
}
