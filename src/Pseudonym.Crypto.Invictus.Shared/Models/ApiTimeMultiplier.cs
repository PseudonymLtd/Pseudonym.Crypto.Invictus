using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiTimeMultiplier
    {
        [Required]
        [JsonProperty("range_min")]
        public int RangeMin { get; set; }

        [Required]
        [JsonProperty("range_max")]
        public int RangeMax { get; set; }

        [Required]
        [JsonProperty("multiplier")]
        public decimal Multiplier { get; set; }
    }
}
