using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiAsset
    {
        [Required]
        [JsonProperty("holding")]
        public ApiHolding Holding { get; set; }

        [Required]
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [JsonProperty("price_per_token")]
        public decimal PricePerToken { get; set; }

        [Required]
        [JsonProperty("total")]
        public decimal Total { get; set; }

        [Required]
        [JsonProperty("share")]
        public decimal Share { get; set; }
    }
}
