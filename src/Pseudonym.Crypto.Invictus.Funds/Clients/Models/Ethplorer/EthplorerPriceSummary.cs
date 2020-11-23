using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer
{
    public sealed class EthplorerPriceSummary
    {
        [JsonProperty("marketCapUsd")]
        public decimal MarketCap { get; set; }

        [JsonProperty("rate")]
        public decimal MarketValuePerToken { get; set; }

        [JsonProperty("diff")]
        public decimal DiffDaily { get; set; }

        [JsonProperty("diff7d")]
        public decimal DiffWeekly { get; set; }

        [JsonProperty("diff30d")]
        public decimal DiffMonthly { get; set; }

        [JsonProperty("ts")]
        public int TransactionCount { get; set; }

        [JsonProperty("availableSupply")]
        public decimal CirculatingSupply { get; set; }

        [JsonProperty("volume24h")]
        public decimal Volume { get; set; }

        [JsonProperty("volDiff1")]
        public decimal VolumeDiffDaily { get; set; }

        [JsonProperty("volDiff7")]
        public decimal VolumeDiffWeekly { get; set; }

        [JsonProperty("volDiff30")]
        public decimal VolumeDiffMonthly { get; set; }
    }
}
