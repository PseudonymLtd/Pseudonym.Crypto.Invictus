using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IStake
    {
        string Name { get; }

        string DisplayName { get; }

        string Description { get; }

        Uri InvictusUri { get; }

        Uri FactSheetUri { get; }

        Uri PoolUri { get; }

        IToken Token { get; }

        decimal CirculatingSupply { get; }

        IMarket Market { get; }

        EthereumAddress StakingAddress { get; }

        IReadOnlyList<ITimeMultiplier> TimeMultipliers { get; }

        IReadOnlyDictionary<Symbol, decimal> FundMultipliers { get; }

        IStakingPower Power { get; }
    }
}
