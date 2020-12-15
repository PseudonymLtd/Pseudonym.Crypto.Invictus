using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessSubInvestment : ISubInvestment
    {
        public ICoin Coin { get; set; }

        public decimal Held { get; set; }

        public decimal MarketValue { get; set; }
    }
}
