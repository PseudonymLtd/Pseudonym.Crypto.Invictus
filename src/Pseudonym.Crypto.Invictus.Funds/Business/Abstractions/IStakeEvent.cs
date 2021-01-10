using System;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IStakeEvent
    {
        DateTime ConfirmedAt { get; }

        EthereumAddress UserAddress { get; }

        EthereumAddress StakeAddress { get; }

        EthereumAddress ContractAddress { get; }

        EthereumTransactionHash Hash { get; }

        StakeEventType Type { get; }

        decimal Change { get; }

        IStakeLock Lock { get; }

        IStakeRelease Release { get; }
    }

    public interface IStakeLock
    {
        TimeSpan Duration { get; }

        DateTime ExpiresAt { get; }

        decimal Quantity { get; }
    }

    public interface IStakeRelease
    {
        decimal Quantity { get; }

        decimal? FeeQuantity { get; }
    }
}
