using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients
{
    internal sealed class CurrencyClient : ICurrencyClient
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CurrencyClient(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<CurrencyRates> GetRatesAsync(CancellationToken cancellationToken)
        {
            using var client = httpClientFactory.CreateClient(nameof(CurrencyClient));

            var response = await client.GetAsync(new Uri("/v6/latest", UriKind.Relative), cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CurrencyRates>(json);
        }
    }
}
