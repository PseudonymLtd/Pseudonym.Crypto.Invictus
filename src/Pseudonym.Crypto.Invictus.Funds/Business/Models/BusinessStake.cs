using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessStake : IStake
    {
        public EthereumAddress ContractAddress { get; set; }

        public EthereumTransactionHash Hash { get; set; }

        public DateTime StakedAt { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime ExpiresAt { get; set; }

        public decimal PricePerToken { get; set; }

        public decimal Quantity { get; set; }
    }
}
