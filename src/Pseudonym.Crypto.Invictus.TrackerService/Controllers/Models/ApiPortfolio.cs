using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models
{
    public sealed class ApiPortfolio
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("currency")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyCode Currency { get; set; }

        [JsonProperty("total_asset_value")]
        public decimal TotalAssetValue => Investments.Sum(x => x.RealValue);

        [JsonProperty("total_market_value")]
        public decimal TotalMarketValue => Investments.Sum(x => x.MarketValue ?? 0);

        [JsonProperty("investments")]
        public List<ApiInvestment> Investments { get; set; } = new List<ApiInvestment>();
    }
}
