using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Funds.Utils;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessTransaction : ITransaction
    {
        public EthereumAddress Address { get; set; }

        public EthereumTransactionHash Hash { get; set; }

        public long BlockNumber { get; set; }

        public long Confirmations { get; set; }

        public DateTime ConfirmedAt { get; set; }

        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public decimal EthValue { get; set; }

        public long GasLimit { get; set; }

        public long GasUsed { get; set; }

        public bool Success { get; set; }

        public string Input { get; set; }

        [DynamoDbIgnore]
        public IReadOnlyList<IOperation> Operations { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BusinessTransaction t &&
                t.Hash.Equals(Hash);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash);
        }
    }
}
