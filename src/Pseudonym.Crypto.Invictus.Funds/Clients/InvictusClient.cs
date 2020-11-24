using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class InvictusClient : IInvictusClient
    {
        private readonly AppSettings appSettings;
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public InvictusClient(
            IOptions<AppSettings> appSettings,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings.Value;
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public async IAsyncEnumerable<InvictusFund> ListFundsAsync()
        {
            var response = await GetAsync<ListFundsResponse>("/v2/funds");

            foreach (var item in response.Funds)
            {
                yield return Format(item);
            }
        }

        public async IAsyncEnumerable<InvictusPerformanceSummary> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to)
        {
            var fundInfo = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol);
            if (fundInfo != null)
            {
                var response = await GetAsync<ListPerformanceResponse>($"/v2/funds/{fundInfo.FundName}/history?start={from}&end={to}");

                foreach (var itemGroup in response.Performance.GroupBy(x => x.Date.Date))
                {
                    var values = itemGroup.Select(x => new
                    {
                        x.Date,
                        Nav = x.NetAssetValuePerToken.FromPythonString(),
                    });

                    yield return new InvictusPerformanceSummary()
                    {
                        Date = itemGroup.Key,
                        NetValue = itemGroup.Average(x => x.NetValue.FromPythonString()),
                        Average = values.Average(x => x.Nav),
                        Open = values.OrderBy(x => x.Date).First().Nav,
                        Close = values.OrderBy(x => x.Date).Last().Nav,
                        High = values.Select(x => x.Nav).Max(),
                        Low = values.Select(x => x.Nav).Min()
                    };
                }
            }
            else
            {
                throw new PermanentException($"Could not find invictus fund with symbol `{symbol}`");
            }
        }

        public async Task<InvictusFund> GetFundAsync(Symbol symbol)
        {
            var fundInfo = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol);
            if (fundInfo != null)
            {
                var fund = await GetAsync<InvictusFund>($"/v2/funds/{fundInfo.FundName}/nav");

                return Format(fund);
            }
            else
            {
                throw new PermanentException($"Could not find invictus fund with symbol `{symbol}`");
            }
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            try
            {
                using var client = httpClientFactory.CreateClient(nameof(InvictusClient));

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

        private InvictusFund Format(InvictusFund item)
        {
            if (item.Symbol.Equals(Symbol.C10.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                item.Name += " Hedged";
            }

            return item;
        }
    }
}
