using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

#pragma warning disable SA1402

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
            var attempts = 1;
            using var client = httpClientFactory.CreateClient(nameof(EtherscanClient));

            while (attempts < 3)
            {
                var response = await client.GetAsync(
                    new Uri(
                        string.Format(
                            "/api?module=account&action=tokenbalance&contractaddress={0}&address={1}&tag=latest&apikey={2}",
                            token.ContractAddress,
                            etherAddress,
                            appSettings.EtherscanApiKey),
                        UriKind.Relative),
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (result.Length > token.Decimals)
                    {
                        var decimalValue = result.Insert(result.Length - token.Decimals, ".");

                        return decimal.Parse(decimalValue);
                    }

                    break;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    attempts++;
                    await Task.Delay(500);
                }
                else
                {
                    throw new HttpRequestException($"Response status code did not indicate success {(int)response.StatusCode}");
                }
            }

            return 0.0m;
        }
    }

    internal sealed class ResponseTranslationHandler : DelegatingHandler
    {
        private const string StatusKey = "status";
        private const string ResultKey = "result";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            var jObj = JObject.Parse(await response.Content.ReadAsStringAsync());

            if (jObj.ContainsKey(StatusKey) && jObj[StatusKey].ToString() == "1")
            {
                response.Content = new StringContent(jObj[ResultKey].ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain);
            }
            else
            {
                response.Content = null;
                response.StatusCode = jObj[ResultKey].ToString().ToLower().Contains("rate limit")
                    ? HttpStatusCode.TooManyRequests
                    : HttpStatusCode.BadRequest;
            }

            return response;
        }
    }
}
