using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiOperation
    {
        [Required]
        [JsonProperty("type")]
        public string Type { get; set; }

        [Required]
        [JsonProperty("raw_value")]
        public string Value { get; set; }

        [Required]
        [JsonProperty("price_per_token")]
        public decimal PricePerToken { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [JsonProperty("is_eth")]
        public bool IsEth { get; set; }

        [Required]
        [JsonProperty("priority")]
        public int Priority { get; set; }

        [EthereumAddress]
        [JsonProperty("sender")]
        public string Sender { get; set; }

        [EthereumAddress]
        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [EthereumAddress]
        [JsonProperty("address")]
        public string Address { get; set; }

        [Required]
        [JsonProperty("contract")]
        public ApiContract Contract { get; set; }
    }
}
