using System.Net.Http;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class CurrencyClient : BaseHttpClient, ICurrencyClient
    {
        public CurrencyClient(
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
            : base(scopedCancellationToken, httpClientFactory)
        {
        }

        public Task<CurrencyRates> GetRatesAsync()
        {
            return GetAsync<CurrencyRates>("/v6/latest");
        }
    }
}
