using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Etherscan;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class EtherscanClient : IEtherscanClient
    {
        private const int PageSize = 100;

        private readonly Dependencies dependencies;
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public EtherscanClient(
            IOptions<Dependencies> dependencies,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.dependencies = dependencies.Value;
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public async IAsyncEnumerable<EtherscanTransaction> ListTransactionsAsync(EthereumAddress contractAddress, long startblock, long endblock)
        {
            var page = 1;

            while (!scopedCancellationToken.Token.IsCancellationRequested)
            {
                var response = await GetAsync<EtherscanTransactionListResponse>(
                    $"/api?module=account&action=txlist&address={contractAddress}&startblock={startblock}&endblock={endblock}&sort=asc&page={page++}&offset={PageSize}");

                if (response.Transactions.Any())
                {
                    foreach (var transaction in response.Transactions)
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
                using var client = httpClientFactory.CreateClient(nameof(EtherscanClient));

                var pathAndQuery = QueryHelpers.AddQueryString(url, "apikey", dependencies.Etherscan.Settings.ApiKey);

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
