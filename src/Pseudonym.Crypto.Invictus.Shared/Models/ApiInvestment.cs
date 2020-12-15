using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiInvestment
    {
        [Required]
        [JsonProperty("fund")]
        public ApiFund Fund { get; set; }

        [Required]
        [JsonProperty("held")]
        public decimal Held { get; set; }

        [Required]
        [JsonProperty("asset_value")]
        public decimal RealValue { get; set; }

        [Required]
        [JsonProperty("market_value")]
        public decimal? MarketValue { get; set; }

        [Required]
        [JsonProperty("share")]
        public decimal Share { get; set; }

        [Required]
        [JsonProperty("sub_investments")]
        public IReadOnlyList<ApiSubInvestment> SubInvestments { get; set; } = new List<ApiSubInvestment>();
    }
}
