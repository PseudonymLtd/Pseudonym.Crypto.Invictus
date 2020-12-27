using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessInvestment : IInvestment
    {
        public IFund Fund { get; set; }

        public decimal Held { get; set; }

        public decimal Share => Held / Fund.CirculatingSupply * 100;

        public decimal RealValue => Fund.NetAssetValuePerToken * Held;

        public decimal? MarketValue => Fund.Market.IsTradable
            ? Fund.Market.PricePerToken * Held
            : default(decimal?);

        public IReadOnlyList<ISubInvestment> SubInvestments { get; set; }

        public IReadOnlyList<IStake> Stakes { get; set; }
    }
}
