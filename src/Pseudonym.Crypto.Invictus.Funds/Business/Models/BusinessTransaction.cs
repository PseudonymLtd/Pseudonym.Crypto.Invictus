using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal class BusinessTransaction : ITransaction
    {
        public EthereumAddress Address { get; set; }

        public EthereumTransactionHash Hash { get; set; }

        public EthereumTransactionHash BlockHash { get; set; }

        public long Nonce { get; set; }

        public long BlockNumber { get; set; }

        public long Confirmations { get; set; }

        public DateTime ConfirmedAt { get; set; }

        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public decimal Eth { get; set; }

        public long Gas { get; set; }

        public long GasLimit { get; set; }

        public decimal GasPrice { get; set; }

        public decimal GasUsed => ((decimal)Gas) / GasLimit * 100;

        public bool Success { get; set; }

        public string Input { get; set; }

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
