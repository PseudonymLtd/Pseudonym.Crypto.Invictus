using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStakingPowerFund
    {
        [Required]
        [JsonProperty("symbol")]
        public Symbol Symbol { get; set; }

        [Required]
        [JsonProperty("modified_quantity")]
        public decimal ModifiedQuantity { get; set; }

        [Required]
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [JsonProperty("power")]
        public decimal Power { get; set; }
    }
}
