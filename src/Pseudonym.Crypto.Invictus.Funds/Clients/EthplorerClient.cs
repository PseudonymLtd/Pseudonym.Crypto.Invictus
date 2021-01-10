using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class EthplorerClient : BaseHttpClient, IEthplorerClient
    {
        private readonly Dependencies dependencies;

        public EthplorerClient(
            IOptions<Dependencies> dependencies,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
            : base(scopedCancellationToken, httpClientFactory)
        {
            this.dependencies = dependencies.Value;
        }

        protected override Func<string, string> AugmentUriFunc =>
            uri => QueryHelpers.AddQueryString(uri, "apiKey", dependencies.Ethplorer.Settings.ApiKey);

        public Task<EthplorerTokenInfo> GetTokenInfoAsync(EthereumAddress contractAddress)
        {
            return GetAsync<EthplorerTokenInfo>($"/getTokenInfo/{contractAddress}");
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
    }
}
