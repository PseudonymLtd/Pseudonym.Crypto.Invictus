using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiContract
    {
        [Required]
        [EthereumAddress]
        [JsonProperty("address")]
        public string Address { get; set; }

        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [Required]
        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [Required]
        [JsonProperty("holders")]
        public long Holders { get; set; }

        [Required]
        [JsonProperty("issuances")]
        public long Issuances { get; set; }

        [Required]
        [JsonProperty("links")]
        public ApiAssetLinks Links { get; set; }
    }
}
