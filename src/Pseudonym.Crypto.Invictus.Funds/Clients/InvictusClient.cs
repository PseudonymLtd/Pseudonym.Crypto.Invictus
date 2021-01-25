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
    internal sealed class InvictusClient : BaseHttpClient, IInvictusClient
    {
        private readonly AppSettings appSettings;

        public InvictusClient(
            IOptions<AppSettings> appSettings,
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
            : base(scopedCancellationToken, httpClientFactory)
        {
            this.appSettings = appSettings.Value;
        }

        public async IAsyncEnumerable<IInvictusFund> ListFundsAsync()
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
                var response = await GetAsync<GetFundResponse>(
                    $"/v2/funds/{fundInfo.Name}/assets-history?start={from.ToISO8601String()}&end={to.ToISO8601String()}&points={(int)(to - from).TotalHours}");

                foreach (var perfSet in response.Data
                    .Where(x => x.Date >= from && x.Date <= to)
                    .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, 0, 0, DateTimeKind.Utc))
                    .OrderBy(x => x.Key))
                {
                    yield return new InvictusPerformance()
                    {
                        Date = perfSet.Key,
                        NetValue = perfSet.Average(x => x.NetValue.FromPythonString()),
                        NetAssetValuePerToken = perfSet.Average(x => x.NetAssetValuePerToken.FromPythonString()),
                        CirculatingSupply = perfSet.OrderBy(x => x.Date).Last().CirculatingSupply.FromPythonString(),
                    };
                }
            }
            else
            {
                throw new PermanentException($"Could not find invictus fund with symbol `{symbol}`");
            }
        }

        public async Task<IInvictusFund> GetFundAsync(Symbol symbol)
        {
            var fundInfo = appSettings.Funds.SingleOrDefault(x => x.Symbol == symbol);
            if (fundInfo != null)
            {
                var date = DateTime.UtcNow;

                var fund = await GetAsync<GetFundResponse>(
                    $"/v2/funds/{fundInfo.Name}/assets-history?start={date.AddHours(-1).ToISO8601String()}&end={date.AddHours(1).ToISO8601String()}&points=1&summary=false");

                var data = fund.Data.Last();

                data.Symbol = symbol;
                data.Name = symbol == Symbol.C10
                    ? $"{fundInfo.Name} Hedged"
                    : fundInfo.Name;

                return data;
            }
            else
            {
                throw new PermanentException($"Could not find invictus fund with symbol `{symbol}`");
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
