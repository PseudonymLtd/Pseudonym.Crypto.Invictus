using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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

        public async IAsyncEnumerable<InvictusPerformance> ListPerformanceAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to)
        {
            var fundInfo = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol);
            if (fundInfo != null)
            {
                var response = await GetAsync<ListPerformanceResponse>(
                    $"/v2/funds/{fundInfo.FundName}/history?start={from.ToISO8601String()}&end={to.AddDays(1).ToISO8601String()}");

                if (response.Performance.Any())
                {
                    var start = response.Performance.Min(x => x.Date.Date);
                    var end = response.Performance.Max(x => x.Date.Date);

                    while (start < end)
                    {
                        if (!response.Performance.Any(x => x.Date.Date == start))
                        {
                            var previousDate = response.Performance
                                .Where(x => x.Date.Date < start)
                                .OrderByDescending(x => x.Date)
                                .FirstOrDefault();

                            var nextDate = response.Performance
                                .Where(x => x.Date.Date > start)
                                .OrderBy(x => x.Date)
                                .FirstOrDefault();

                            if (previousDate != null && nextDate == null)
                            {
                                response.Performance.Add(new InvictusPerformance()
                                {
                                    Date = new DateTimeOffset(start.AddHours(12), TimeSpan.Zero),
                                    NetValue = previousDate.NetValue,
                                    NetAssetValuePerToken = previousDate.NetAssetValuePerToken
                                });
                            }
                            else if (previousDate == null && nextDate != null)
                            {
                                response.Performance.Add(new InvictusPerformance()
                                {
                                    Date = new DateTimeOffset(start.AddHours(12), TimeSpan.Zero),
                                    NetValue = nextDate.NetValue,
                                    NetAssetValuePerToken = nextDate.NetAssetValuePerToken
                                });
                            }
                            else
                            {
                                response.Performance.Add(new InvictusPerformance()
                                {
                                    Date = new DateTimeOffset(start.AddHours(12), TimeSpan.Zero),
                                    NetValue = ((nextDate.NetValue.FromPythonString() + previousDate.NetValue.FromPythonString()) / 2).ToString(),
                                    NetAssetValuePerToken = ((nextDate.NetAssetValuePerToken.FromPythonString() + previousDate.NetAssetValuePerToken.FromPythonString()) / 2).ToString()
                                });
                            }
                        }

                        start = start.AddDays(1);
                    }
                }

                foreach (var perfSet in response.Performance
                    .Where(x => x.Date >= from && x.Date <= to)
                    .GroupBy(x => new DateTimeOffset(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, 0, 0, TimeSpan.Zero))
                    .OrderBy(x => x.Key))
                {
                    yield return new InvictusPerformance()
                    {
                        Date = perfSet.Key,
                        NetValue = perfSet.Average(x => x.NetValue.FromPythonString()).ToString(),
                        NetAssetValuePerToken = perfSet.Average(x => x.NetAssetValuePerToken.FromPythonString()).ToString()
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

                return json.Deserialize<TResponse>();
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
