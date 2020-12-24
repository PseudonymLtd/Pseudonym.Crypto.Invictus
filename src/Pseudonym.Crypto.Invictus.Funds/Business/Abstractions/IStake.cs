using System;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IStake
    {
        EthereumAddress ContractAddress { get; }

        EthereumTransactionHash Hash { get; }

        DateTime StakedAt { get; }

        TimeSpan Duration { get; }

        DateTime ExpiresAt { get; }

        decimal PricePerToken { get; }

        decimal Quantity { get; }
    }
}
