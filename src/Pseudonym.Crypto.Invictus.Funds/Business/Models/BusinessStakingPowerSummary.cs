using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessStakingPowerSummary : IStakingPowerSummary
    {
        public Symbol Symbol { get; set; }

        public decimal Power { get; set; }
    }
}
