﻿using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Models
{
    internal sealed class BusinessInvestment : IInvestment
    {
        public IFund Fund { get; set; }

        public decimal Held { get; set; }

        public decimal Share => Held / Fund.CirculatingSupply * 100;

        public decimal RealValue => Fund.NetAssetValuePerToken * Held;

        public decimal? MarketValue => Fund.MarketValuePerToken.HasValue
                ? Fund.MarketValuePerToken.Value * Held
                : default(decimal?);
    }
}
