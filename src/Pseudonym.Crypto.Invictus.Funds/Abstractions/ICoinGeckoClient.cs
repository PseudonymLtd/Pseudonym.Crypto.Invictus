using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface ICoinGeckoClient
    {
        IAsyncEnumerable<CoinGeckoCoin> ListCoinsAsync();

        Task<CoinGeckoCoinInformationResponse> GetCoinAsync(string coinId);

        IAsyncEnumerable<CoinGeckoCoinPerformance> ListCoinPerformanceAsync(string coinId, DateTimeOffset from, DateTimeOffset to);
    }
}
