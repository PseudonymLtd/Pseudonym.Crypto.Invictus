using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Bloxy;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class BloxyClient : IBloxyClient
    {
        private const int PageSize = 1000;

        private readonly Dependencies dependencies;
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public BloxyClient(
            IOptions<Dependencies> dependencies,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.dependencies = dependencies.Value;
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public async IAsyncEnumerable<BloxyTokenTransfer> ListTransactionsAsync(EthereumAddress contractAddress, DateTime startDate, DateTime endDate)
        {
            var page = 0;

            while (!scopedCancellationToken.Token.IsCancellationRequested)
            {
                var transactions = await GetAsync<List<BloxyTokenTransfer>>(
                    $"/token/transfers?token={contractAddress}&limit={PageSize}&offset={page++ * PageSize}&from_time={startDate.ToISO8601String()}&till_time={endDate.ToISO8601String()}");

                if (transactions.Any())
                {
                    foreach (var transaction in transactions.Distinct())
                    {
                        yield return transaction;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(nameof(BloxyClient));

                var pathAndQuery = QueryHelpers.AddQueryString(url, "key", dependencies.Bloxy.Settings.ApiKey);

                var response = await client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), scopedCancellationToken.Token);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TResponse>(json);
            }
            catch (HttpRequestException e)
            {
                throw new TransientException($"{GetType().Name} Error calling GET {url}", e);
            }
        }
    }
}
