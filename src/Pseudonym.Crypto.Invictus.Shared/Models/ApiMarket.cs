using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Models.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiMarket : IPricing
    {
        [Required]
        [JsonProperty("is_tradable")]
        public bool IsTradeable { get; set; }

        [Required]
        [JsonProperty("market_cap")]
        public decimal Cap { get; set; }

        [Required]
        [JsonProperty("market_total")]
        public decimal Total { get; set; }

        [Required]
        [JsonProperty("market_price_per_token")]
        public decimal PricePerToken { get; set; }

        [Required]
        [JsonProperty("market_diff_daily")]
        public decimal DiffDaily { get; set; }

        [Required]
        [JsonProperty("market_diff_weekly")]
        public decimal DiffWeekly { get; set; }

        [Required]
        [JsonProperty("market_diff_monthly")]
        public decimal DiffMonthly { get; set; }

        [Required]
        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [Required]
        [JsonProperty("volume_diff_daily")]
        public decimal VolumeDiffDaily { get; set; }

        [Required]
        [JsonProperty("volume_diff_weekly")]
        public decimal VolumeDiffWeekly { get; set; }

        [Required]
        [JsonProperty("volume_diff_monthly")]
        public decimal VolumeDiffMonthly { get; set; }
    }
}
