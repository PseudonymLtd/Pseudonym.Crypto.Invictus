using System.Collections.Generic;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public class UniswapPerformanceData
    {
        [JsonRequired]
        [JsonProperty("tokenDayDatas")]
        public List<UniswapPerformance> Performance { get; set; }
    }

    public sealed class UniswapPerformance
    {
        [JsonRequired]
        [JsonProperty("date")]
        public long UnixDate { get; set; }

        [JsonRequired]
        [JsonProperty("priceUSD")]
        public string Price { get; set; }

        [JsonRequired]
        [JsonProperty("totalLiquidityUSD")]
        public string MarketCap { get; set; }

        [JsonRequired]
        [JsonProperty("dailyVolumeUSD")]
        public string Volume { get; set; }
    }
}
