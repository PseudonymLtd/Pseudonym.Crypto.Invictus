using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStakeEvent
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
        [JsonProperty("confirmed_at")]
        public DateTime ConfirmedAt { get; set; }

        [Required]
        [JsonProperty("type")]
        public StakeEventType Type { get; set; }

        [Required]
        [JsonProperty("change")]
        public decimal Change { get; set; }

        [JsonProperty("lock")]
        public ApiStakeLock Lock { get; set; }

        [JsonProperty("release")]
        public ApiStakeRelease Release { get; set; }
    }

    public sealed class ApiStakeLock
    {
        [Required]
        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }

        [Required]
        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Required]
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }
    }

    public sealed class ApiStakeRelease
    {
        [Required]
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("fee_quantity")]
        public decimal? FeeQuantity { get; set; }
    }
}
