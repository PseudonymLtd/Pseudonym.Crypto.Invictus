using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiInvestment
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("held")]
        public decimal Held { get; set; }

        [Required]
        [JsonProperty("asset_value")]
        public decimal RealValue { get; set; }

        [Required]
        [JsonProperty("market_value")]
        public decimal? MarketValue { get; set; }

        [Required]
        [JsonProperty("share")]
        public decimal Share { get; set; }
    }
}
