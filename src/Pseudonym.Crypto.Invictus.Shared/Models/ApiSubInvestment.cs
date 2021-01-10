using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiSubInvestment
    {
        [Required]
        [JsonProperty("coin")]
        public ApiHolding Coin { get; set; }

        [Required]
        [JsonProperty("held")]
        public decimal Held { get; set; }

        [Required]
        [JsonProperty("market_value")]
        public decimal MarketValue { get; set; }
    }
}
