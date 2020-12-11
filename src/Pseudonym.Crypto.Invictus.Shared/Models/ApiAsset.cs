using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiAsset
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [Required]
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [Required]
        [JsonProperty("share")]
        public decimal Share { get; set; }

        [Required]
        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("coin_id")]
        public string CoinId { get; set; }
    }
}
