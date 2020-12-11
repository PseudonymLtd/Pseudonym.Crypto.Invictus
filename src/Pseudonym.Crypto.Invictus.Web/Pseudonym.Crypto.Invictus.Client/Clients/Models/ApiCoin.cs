using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Web.Client.Clients.Models
{
    public sealed class ApiCoin
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameid")]
        public string NameId { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("price_usd")]
        public string PriceUSD { get; set; }

        [JsonProperty("percent_change_24h")]
        public string PercentChangeDaily { get; set; }

        [JsonProperty("percent_change_1h")]
        public string PercentChangeHourly { get; set; }

        [JsonProperty("percent_change_7d")]
        public string PercentChangeWeekly { get; set; }

        [JsonProperty("market_cap_usd")]
        public string MarketCap { get; set; }

        [JsonProperty("t24h_volume_usd")]
        public string VolumeUSD { get; set; }

        [JsonProperty("t24h_volume_native")]
        public string VolumeNative { get; set; }

        [JsonProperty("mc")]
        public string CurrencyCode { get; set; }

        [JsonProperty("price_btc")]
        public string PriceBTC { get; set; }
    }
}
