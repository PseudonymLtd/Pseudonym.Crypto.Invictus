using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public enum TransferType
    {
        IN,
        OUT
    }

    public sealed class ApiTransaction
    {
        [Required]
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [Required]
        [JsonProperty("mined_at")]
        public DateTime MinedAt { get; set; }

        [Required]
        [JsonProperty("sender")]
        public string Sender { get; set; }

        [Required]
        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [Required]
        [JsonProperty("type")]
        public TransferType Type { get; set; }

        [Required]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
