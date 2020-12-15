using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiCoin
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("decimals")]
        public int? Decimals { get; set; }

        [JsonProperty("contract_address")]
        public string ContractAddress { get; set; }

        [JsonProperty("fixed_value_per_coin")]
        public decimal? FixedValuePerCoin { get; set; }

        [JsonProperty("hex_colour")]
        public string HexColour { get; set; }

        [Required]
        [JsonProperty("links")]
        public ApiCoinLinks Links { get; set; }
    }
}
