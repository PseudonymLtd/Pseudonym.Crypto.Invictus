using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IGraphClient
    {
        Task<UniswapPairResult> GetUniswapPairAsync(EthereumAddress pairAddress);

        Task<decimal> GetUniswapPriceAsync(EthereumAddress pairAddress, EthereumAddress contractAddress);

        IAsyncEnumerable<UniswapTokenPerformanceResult> ListUniswapTokenPerformanceAsync(EthereumAddress contractAddress, DateTimeOffset from, DateTimeOffset to);
    }
}
