using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiToken
    {
        [Required]
        [JsonProperty("symbol")]
        public Symbol Symbol { get; set; }

        [Required]
        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [Required]
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
