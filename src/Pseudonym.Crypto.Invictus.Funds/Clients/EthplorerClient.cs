using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class EthplorerClient : IEthplorerClient
    {
        private readonly Dependencies dependencies;
        private readonly IScopedCorrelation scopedCorrelation;
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public EthplorerClient(
            IOptions<Dependencies> dependencies,
            IScopedCorrelation scopedCorrelation,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.dependencies = dependencies.Value;
            this.scopedCorrelation = scopedCorrelation;
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<EthplorerPriceSummary> GetTokenInfoAsync(EthereumAddress contractAddress)
        {
            var response = await GetAsync<EthplorerTokenInfo>($"/getTokenInfo/{contractAddress}");

            return response.Price;
        }

        public async Task<EthplorerPriceData> GetTokenPricingAsync(EthereumAddress contractAddress)
        {
            var response = await GetAsync<EthplorerPriceResponse>($"/getTokenPriceHistoryGrouped/{contractAddress}");

            return response.Data;
        }

        public Task<EthplorerTransaction> GetTransactionAsync(EthereumTransactionHash hash)
        {
            return GetAsync<EthplorerTransaction>($"/getTxInfo/{hash}");
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(nameof(EthplorerClient));

                var pathAndQuery = QueryHelpers.AddQueryString(url, "apiKey", dependencies.Ethplorer.Settings.ApiKey);

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
