using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models
{
    public sealed class ApiInvestment
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("held")]
        public decimal Held { get; set; }

        [JsonProperty("asset_value")]
        public decimal RealValue { get; set; }

        [JsonProperty("market_value")]
        public decimal? MarketValue { get; set; }

        [JsonProperty("share")]
        public decimal Share { get; set; }
    }
}
