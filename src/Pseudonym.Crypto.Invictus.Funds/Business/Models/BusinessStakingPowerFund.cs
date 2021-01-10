using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessStakingPowerFund : IStakingPowerFund
    {
        public Symbol Symbol { get; set; }

        public decimal FundModifier { get; set; }

        public decimal Quantity { get; set; }

        public decimal ModifiedQuantity { get; set; }

        public decimal Power { get; set; }
    }
}
