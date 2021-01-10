using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessStakingPower : IStakingPower
    {
        public Symbol Symbol { get; set; }

        public DateTime Date { get; set; }

        public decimal Power { get; set; }

        public IReadOnlyList<IStakingPowerSummary> Summary { get; set; } = new List<IStakingPowerSummary>();

        public IReadOnlyList<IStakingPowerFund> Breakdown { get; set; } = new List<IStakingPowerFund>();
    }
}
