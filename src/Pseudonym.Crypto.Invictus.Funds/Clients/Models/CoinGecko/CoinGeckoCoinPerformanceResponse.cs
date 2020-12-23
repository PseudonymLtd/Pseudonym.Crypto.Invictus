using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko
{
    public sealed class CoinGeckoCoinPerformanceResponse
    {
        [JsonProperty("prices")]
        public List<decimal[]> Prices { get; set; }

        [JsonProperty("market_caps")]
        public List<decimal[]> MarketCaps { get; set; }

        [JsonProperty("total_volumes")]
        public List<decimal[]> Volumes { get; set; }
    }

    public sealed class CoinGeckoCoinPerformance
    {
        public DateTimeOffset Date { get; set; }

        public decimal Price { get; set; }

        public decimal MarketCap { get; set; }

        public decimal Volume { get; set; }
    }
}
