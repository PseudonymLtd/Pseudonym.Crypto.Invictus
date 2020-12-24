using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStake
    {
        [Required]
        [TransactionHash]
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [Required]
        [EthereumAddress]
        [JsonProperty("contract_address")]
        public string ContractAddress { get; set; }

        [Required]
        [JsonProperty("staked_at")]
        public DateTime StakedAt { get; set; }

        [Required]
        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }

        [Required]
        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Required]
        [JsonProperty("price_per_token")]
        public decimal? PricePerToken { get; set; }

        [Required]
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [JsonProperty("total")]
        public decimal? Total { get; set; }
    }
}
