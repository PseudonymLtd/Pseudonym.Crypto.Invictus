using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients
{
    internal sealed class EtherscanClient : IEtherscanClient
    {
        private readonly AppSettings appSettings;
        private readonly IHttpClientFactory httpClientFactory;

        public EtherscanClient(
            IOptions<AppSettings> appSettings,
            IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings.Value;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<decimal> GetTokensForAddressAsync(IToken token, EthereumAddress etherAddress, CancellationToken cancellationToken)
        {
            using var client = httpClientFactory.CreateClient(nameof(EtherscanClient));

            var response = await client.GetAsync(
                new Uri(
                    string.Format(
                        "/api?module=account&action=tokenbalance&contractaddress={0}&address={1}&tag=latest&apikey={2}",
                        token.ContractAddress,
                        etherAddress,
                        appSettings.EtherscanApiKey),
                    UriKind.Relative),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var jObj = JObject.Parse(await response.Content.ReadAsStringAsync());

            var result = jObj["result"].ToString();

            if (result.Length > token.Decimals)
            {
                var decimalValue = result.Insert(result.Length - token.Decimals, ".");

                return decimal.Parse(decimalValue);
            }
            else
            {
                return 0.0m;
            }
        }
    }
}
