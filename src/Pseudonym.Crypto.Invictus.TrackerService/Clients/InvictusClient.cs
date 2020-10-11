﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Models.Exceptions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients
{
    internal sealed class InvictusClient : IInvictusClient
    {
        private readonly AppSettings appSettings;
        private readonly IScopedCorrelation scopedCorrelation;
        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly IHttpClientFactory httpClientFactory;

        public InvictusClient(
            IOptions<AppSettings> appSettings,
            IScopedCorrelation scopedCorrelation,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings.Value;
            this.scopedCorrelation = scopedCorrelation;
            this.scopedCancellationToken = scopedCancellationToken;
            this.httpClientFactory = httpClientFactory;
        }

        public async IAsyncEnumerable<InvictusFund> ListFundsAsync()
        {
            var response = await GetAsync<ListFundsResponse>("/v2/funds");

            foreach (var item in response.Funds)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<InvictusPerformance> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to)
        {
            var fundInfo = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol);

            if (fundInfo != null)
            {
                var response = await GetAsync<ListPerformanceResponse>($"/v2/funds/{fundInfo.FundName}/history?start={from}&end={to}");

                foreach (var item in response.Performance)
                {
                    yield return item;
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
                if (symbol == Symbol.C20)
                {
                    var fund = await GetAsync<InvictusC20Fund>("/v2/funds/c20_status");

                    return new InvictusFund()
                    {
                        Symbol = Symbol.C20.ToString(),
                        Name = fund.Name,
                        CirculatingSupply = fund.CirculatingSupply,
                        NetValue = fund.NetValue,
                        NetAssetValuePerToken = fund.NetAssetValuePerToken,
                        MarketValuePerToken = fund.NetAssetValuePerToken, // Same as NAV,
                        Assets = fund.Holdings
                            .Select(h => new InvictusAsset()
                            {
                                Symbol = h.Symbol,
                                Name = h.Name,
                                Value = h.Value
                            })
                            .ToList()
                    };
                }
                else
                {
                    return await GetAsync<InvictusFund>($"/v2/funds/{fundInfo.FundName}/nav");
                }
            }
            else
            {
                throw new PermanentException($"Could not find invictus fund with symbol `{symbol}`");
            }
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : class, new()
        {
            using var client = httpClientFactory.CreateClient(nameof(InvictusClient));

            client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, scopedCorrelation.CorrelationId);

            var response = await client.GetAsync(new Uri(url, UriKind.Relative), scopedCancellationToken.Token);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(json);
        }
    }
}
