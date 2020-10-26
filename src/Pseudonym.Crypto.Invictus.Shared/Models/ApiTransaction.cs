using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiTransaction
    {
        [Required]
        [JsonProperty("sender")]
        public string Sender { get; set; }

        [Required]
        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [Required]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
