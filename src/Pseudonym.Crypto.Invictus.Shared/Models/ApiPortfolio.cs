using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiPortfolio
    {
        [Required]
        [JsonProperty("address")]
        public string Address { get; set; }

        [Required]
        [JsonProperty("currency")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyCode Currency { get; set; }

        [Required]
        [JsonProperty("total_asset_value")]
        public decimal TotalAssetValue => Investments.Sum(x => x.RealValue);

        [Required]
        [JsonProperty("total_market_value")]
        public decimal TotalMarketValue => Investments.Sum(x => x.MarketValue ?? 0);

        [Required]
        [JsonProperty("investments")]
        public List<ApiInvestment> Investments { get; set; } = new List<ApiInvestment>();
    }
}
