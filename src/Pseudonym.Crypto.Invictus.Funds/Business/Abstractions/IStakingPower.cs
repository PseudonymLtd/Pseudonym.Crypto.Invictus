using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IStakingPower
    {
        DateTime Date { get; }

        decimal Power { get; }

        IReadOnlyList<IStakingPowerSummary> Summary { get; }

        IReadOnlyList<IStakingPowerFund> Breakdown { get; }
    }

    public interface IStakingPowerFund
    {
        Symbol Symbol { get; }

        decimal FundModifier { get; }

        decimal Quantity { get; }

        decimal ModifiedQuantity { get; }

        decimal Power { get; }
    }

    public interface IStakingPowerSummary
    {
        Symbol Symbol { get; }

        decimal Power { get; }
    }
}
