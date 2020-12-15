﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiAsset
    {
        [Required]
        [JsonProperty("coin")]
        public ApiCoin Coin { get; set; }

        [Required]
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [Required]
        [JsonProperty("share")]
        public decimal Share { get; set; }
    }
}
