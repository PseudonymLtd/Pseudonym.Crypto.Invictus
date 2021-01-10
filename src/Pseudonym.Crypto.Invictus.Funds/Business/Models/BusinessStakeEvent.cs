using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessStakeEvent : IStakeEvent
    {
        public DateTime ConfirmedAt { get; set; }

        public EthereumAddress UserAddress { get; set; }

        public EthereumAddress StakeAddress { get; set; }

        public EthereumAddress ContractAddress { get; set; }

        public EthereumTransactionHash Hash { get; set; }

        public StakeEventType Type { get; set; }

        public decimal Change => Type == StakeEventType.Lockup
            ? Lock.Quantity
            : -(Release.Quantity + (Release.FeeQuantity ?? decimal.Zero));

        public IStakeLock Lock { get; set; }

        public IStakeRelease Release { get; set; }
    }

    internal sealed class BusinessStakeLock : IStakeLock
    {
        public TimeSpan Duration { get; set; }

        public DateTime ExpiresAt { get; set; }

        public decimal Quantity { get; set; }
    }

    internal sealed class BusinessStakeRelease : IStakeRelease
    {
        public decimal Quantity { get; set; }

        public decimal? FeeQuantity { get; set; }
    }
}
