using System;

namespace Pseudonym.Crypto.Invictus.Funds.Data.Models
{
    public sealed class DataTransaction
    {
        public string Address { get; set; }

        public string Hash { get; set; }

        public long BlockNumber { get; set; }

        public string BlockHash { get; set; }

        public long Nonce { get; set; }

        public long Confirmations { get; set; }

        public DateTime ConfirmedAt { get; set; }

        public string Sender { get; set; }

        public string Recipient { get; set; }

        public decimal Eth { get; set; }

        public long GasLimit { get; set; }

        public long Gas { get; set; }

        public decimal GasPrice { get; set; }

        public bool Success { get; set; }

        public string Input { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DataTransaction t &&
                t.Hash.Equals(Hash);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash);
        }
    }
}
