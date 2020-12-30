using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Models.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiNav : IPricing
    {
        [Required]
        [JsonProperty("net_value")]
        public decimal Value { get; set; }

        [Required]
        [JsonProperty("net_asset_value_per_token")]
        public decimal ValuePerToken { get; set; }

        [Required]
        [JsonProperty("nav_diff_daily")]
        public decimal DiffDaily { get; set; }

        [Required]
        [JsonProperty("nav_diff_weekly")]
        public decimal DiffWeekly { get; set; }

        [Required]
        [JsonProperty("nav_diff_monthly")]
        public decimal DiffMonthly { get; set; }

        [JsonIgnore]
        public decimal Total => Value;

        [JsonIgnore]
        public decimal PricePerToken => ValuePerToken;
    }
}
