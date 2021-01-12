using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Clients
{
    internal sealed class TheGraphClient : BaseHttpClient, IGraphClient
    {
        public TheGraphClient(
            IScopedCancellationToken scopedCancellationToken,
            IHttpClientFactory httpClientFactory)
            : base(scopedCancellationToken, httpClientFactory)
        {
        }

        public async Task<UniswapPairResult> GetUniswapPairAsync(EthereumAddress pairAddress)
        {
            var response = await PostAsync<UniswapPairRequest, UniswapResponse<UniswapPairData>>(
                "/subgraphs/name/uniswap/uniswap-v2",
                new UniswapPairRequest(pairAddress));

            return new UniswapPairResult()
            {
                PoolAddress = pairAddress,
                Volume = response.Data.Pair.Volume.FromBigInteger(),
                Tokens = new List<UniswapTokenResult>()
                {
                    new UniswapTokenResult()
                    {
                        Symbol = response.Data.Pair.Token0.Symbol,
                        Name = response.Data.Pair.Token0.Name,
                        Decimals = int.Parse(response.Data.Pair.Token0.Decimals),
                        ContractAddress = new EthereumAddress(response.Data.Pair.Token0.ContractAddress),
                        PricePerToken = response.Data.Pair.Price0.FromBigInteger(),
                        PoolSupply = response.Data.Pair.Supply0.FromBigInteger()
                    },
                    new UniswapTokenResult()
                    {
                        Symbol = response.Data.Pair.Token1.Symbol,
                        Name = response.Data.Pair.Token1.Name,
                        Decimals = int.Parse(response.Data.Pair.Token1.Decimals),
                        ContractAddress = new EthereumAddress(response.Data.Pair.Token1.ContractAddress),
                        PricePerToken = response.Data.Pair.Price1.FromBigInteger(),
                        PoolSupply = response.Data.Pair.Supply1.FromBigInteger()
                    }
                }
            };
        }

        public async IAsyncEnumerable<UniswapTokenPerformanceResult> ListUniswapTokenPerformanceAsync(EthereumAddress contractAddress, DateTimeOffset from, DateTimeOffset to)
        {
            var response = await PostAsync<UniswapTokenPerformanceRequest, UniswapResponse<UniswapPerformanceData>>(
                "/subgraphs/name/uniswap/uniswap-v2",
                new UniswapTokenPerformanceRequest(contractAddress, from, to));

            if (response.Data.Performance.Any())
            {
                foreach (var perf in response.Data.Performance)
                {
                    yield return new UniswapTokenPerformanceResult()
                    {
                        Date = DateTimeOffset.FromUnixTimeSeconds(perf.UnixDate).UtcDateTime,
                        Price = perf.Price.FromBigInteger(),
                        MarketCap = perf.MarketCap.FromBigInteger(),
                        Volume = perf.Volume.FromBigInteger()
                    };
                }
            }
            else
            {
                yield break;
            }
        }
    }
}