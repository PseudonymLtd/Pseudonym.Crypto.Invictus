using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Models
{
    public sealed class ApiFund
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("token")]
        public ApiToken Token { get; set; }

        [JsonProperty("tradable")]
        public bool IsTradeable { get; set; }

        [JsonProperty("circulating_supply")]
        public decimal CirculatingSupply { get; set; }

        [JsonProperty("net_value")]
        public decimal NetAssetValue { get; set; }

        [JsonProperty("market_value")]
        public decimal? MarketValue { get; set; }

        [JsonProperty("nav_per_token")]
        public decimal NetAssetValuePerToken { get; set; }

        [JsonProperty("market_value_per_token")]
        public decimal? MarketValuePerToken { get; set; }

        [JsonProperty("assets")]
        public List<ApiAsset> Assets { get; set; }
    }
}
