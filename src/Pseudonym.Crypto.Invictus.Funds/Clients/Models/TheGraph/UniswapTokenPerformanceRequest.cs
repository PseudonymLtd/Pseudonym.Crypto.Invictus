using System;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public class UniswapTokenPerformanceRequest
    {
        private const string Template = @"
        {
            tokenDayDatas(orderBy: date, orderDirection: asc,
            where: {
                token: ""%CONTRACT_ADDRESS%"",
                date_gte: %FROM%,
                date_lte: %TO%
            }) 
            {
                date
                priceUSD
                totalLiquidityUSD
                dailyVolumeUSD
            }
        }";

        public UniswapTokenPerformanceRequest(EthereumAddress contractAddress, DateTimeOffset from, DateTimeOffset to)
        {
            Query = Template
                .Replace("%CONTRACT_ADDRESS%", contractAddress.Address)
                .Replace("%FROM%", from.ToUnixTimeSeconds().ToString())
                .Replace("%TO%", to.ToUnixTimeSeconds().ToString());
        }

        [JsonRequired]
        [JsonProperty("query")]
        public string Query { get; private set; }
    }
}
