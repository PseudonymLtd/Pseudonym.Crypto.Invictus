using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using static Pseudonym.Crypto.Invictus.Funds.Configuration.StakeSettings;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions
{
    public interface IStakeSettings
    {
        Symbol Symbol { get; }

        string Name { get; }

        string Description { get; }

        DateTime InceptionDate { get; }

        int Decimals { get; }

        IReadOnlyList<StakeTimeMultiplier> TimeMultipliers { get; }

        EthereumAddress StakingAddress { get; }

        EthereumAddress ContractAddress { get; }

        EthereumAddress PoolAddress { get; }

        IReadOnlyList<EthereumAddress> FeeAddresses { get; }

        IReadOnlyDictionary<Symbol, decimal> FundMultipliers { get; }

        StakeLinks Links { get; }
    }
}
