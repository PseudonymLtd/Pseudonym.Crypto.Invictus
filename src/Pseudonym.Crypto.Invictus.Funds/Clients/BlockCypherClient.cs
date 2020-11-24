using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.BlockCypher;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class BlockCypherClient : IBlockCypherClient
    {
        private const int PageSize = 2000;

        private readonly AppSettings appSettings;
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public BlockCypherClient(
            IOptions<AppSettings> appSettings,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings.Value;
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public Task<BlockCypherAddressResponse> GetAddressInformationAsync(EthereumAddress contractAddress, long beforeBlock, long afterBlock)
        {
            return GetAsync<BlockCypherAddressResponse>($"/v1/eth/main/addrs/{contractAddress}?confirmations=10&before={beforeBlock}&after={afterBlock}&limit={PageSize}");
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(nameof(BlockCypherClient));

                var response = await client.GetAsync(new Uri(url, UriKind.Relative), scopedCancellationToken.Token);

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
