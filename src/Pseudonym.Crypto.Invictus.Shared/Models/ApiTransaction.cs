using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public class ApiTransaction
    {
        [Required]
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [Required]
        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [Required]
        [JsonProperty("block_number")]
        public long BlockNumber { get; set; }

        [Required]
        [JsonProperty("block_hash")]
        public string BlockHash { get; set; }

        [Required]
        [JsonProperty("success")]
        public bool Success { get; set; }

        [Required]
        [JsonProperty("confirmed_at")]
        public DateTime ConfirmedAt { get; set; }

        [Required]
        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }

        [Required]
        [JsonProperty("sender")]
        public string Sender { get; set; }

        [Required]
        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [Required]
        [JsonProperty("eth")]
        public decimal Eth { get; set; }

        [Required]
        [JsonProperty("input")]
        public string Input { get; set; }

        [Required]
        [JsonProperty("gas")]
        public long Gas { get; set; }

        [Required]
        [JsonProperty("gas_limit")]
        public long GasLimit { get; set; }

        [Required]
        [JsonProperty("gas_used")]
        public decimal GasUsed { get; set; }

        [Required]
        [JsonProperty("gas_price")]
        public decimal GasPrice { get; set; }
    }
}
