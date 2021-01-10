using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class CoinGeckoClient : BaseHttpClient, ICoinGeckoClient
    {
        public CoinGeckoClient(
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
            : base(scopedCancellationToken, httpClientFactory)
        {
        }

        public async IAsyncEnumerable<CoinGeckoCoin> ListCoinsAsync()
        {
            var coins = await GetAsync<List<CoinGeckoCoin>>($"/api/v3/coins/list");

            foreach (var coin in coins.Distinct())
            {
                yield return coin;
            }
        }

        public async Task<CoinGeckoCoinInformationResponse> GetCoinAsync(string coinId)
        {
            return await GetAsync<CoinGeckoCoinInformationResponse>(
                $"/api/v3/coins/{coinId}?localization=false&tickers=false&market_data=true&community_data=false&developer_data=false&sparkline=false");
        }

        public async IAsyncEnumerable<CoinGeckoCoinPerformance> ListCoinPerformanceAsync(string coinId, DateTimeOffset from, DateTimeOffset to)
        {
            var response = await GetAsync<CoinGeckoCoinPerformanceResponse>(
                $"/api/v3/coins/{coinId}/market_chart/range?vs_currency=usd&from={from.ToUnixTimeSeconds()}&to={to.ToUnixTimeSeconds()}");

            var priceData = ExpandData(response.Prices);
            var marketCapData = ExpandData(response.MarketCaps);
            var volumeData = ExpandData(response.Volumes);

            foreach (var item in priceData)
            {
                var marketCap = marketCapData.SingleOrDefault(x => x.Date == item.Date);
                var volume = volumeData.SingleOrDefault(x => x.Date == item.Date);

                yield return new CoinGeckoCoinPerformance()
                {
                    Date = item.Date,
                    Price = item.Value,
                    MarketCap = marketCap?.Value ?? 0,
                    Volume = volume?.Value ?? 0,
                };
            }
        }

        private List<TimeValue> ExpandData(IEnumerable<decimal[]> dataSet)
        {
            return dataSet
                .Select(x => new
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)x.First()).Round(),
                    Value = x.Last()
                })
                .GroupBy(x => x.Date)
                .Select(g => new TimeValue()
                {
                    Date = g.Key,
                    Value = g.Average(x => x.Value)
                })
                .ToList();
        }

        private class TimeValue
        {
            public DateTimeOffset Date { get; set; }

            public decimal Value { get; set; }
        }
    }
}
