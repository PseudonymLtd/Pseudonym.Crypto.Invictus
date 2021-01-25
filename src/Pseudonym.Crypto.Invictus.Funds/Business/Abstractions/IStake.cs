using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IStake : IProduct
    {
        Uri PoolUri { get; }

        EthereumAddress StakingAddress { get; }

        IReadOnlyList<ITimeMultiplier> TimeMultipliers { get; }

        IReadOnlyDictionary<Symbol, decimal> FundMultipliers { get; }

        IStakingPower Power { get; }
    }
}
