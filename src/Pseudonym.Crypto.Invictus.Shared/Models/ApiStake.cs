using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStake : IProduct
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [Required]
        [JsonProperty("category")]
        public string Category { get; set; }

        [Required]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Required]
        [JsonProperty("token")]
        public ApiToken Token { get; set; }

        [Required]
        [JsonProperty("circulating_supply")]
        public decimal CirculatingSupply { get; set; }

        [Required]
        [JsonProperty("market")]
        public ApiMarket Market { get; set; }

        [Required]
        [JsonProperty("staking_address")]
        public string StakingAddress { get; set; }

        [Required]
        [JsonProperty("power")]
        public ApiStakingPower Power { get; set; }

        [Required]
        [JsonProperty("time_multipliers")]
        public IReadOnlyList<ApiTimeMultiplier> TimeMultipliers { get; set; }

        [Required]
        [JsonProperty("fund_multipliers")]
        public IReadOnlyDictionary<Symbol, decimal> FundMultipliers { get; set; }

        [Required]
        [JsonProperty("links")]
        public ApiStakeLinks Links { get; set; }

        [JsonIgnore]
        public ApiNav Nav => new ApiNav()
        {
            Value = Market.Total,
            ValuePerToken = Market.PricePerToken,
            DiffDaily = Market.DiffDaily,
            DiffWeekly = Market.DiffWeekly,
            DiffMonthly = Market.DiffMonthly
        };

        [JsonIgnore]
        ILinks IProduct.Links => Links;
    }
}
