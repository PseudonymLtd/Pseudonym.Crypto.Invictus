using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiPerformance
    {
        [Required]
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [Required]
        [JsonProperty("nav_per_token")]
        public decimal NetAssetValuePerToken { get; set; }

        [Required]
        [JsonProperty("net_value")]
        public decimal NetValue { get; set; }
    }
}
