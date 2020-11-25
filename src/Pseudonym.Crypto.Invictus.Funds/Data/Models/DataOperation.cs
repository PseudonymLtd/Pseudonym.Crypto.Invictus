using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Data.Models
{
    public sealed class DataOperation
    {
        public string Hash { get; set; }

        public int Order { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public decimal Price { get; set; }

        public bool IsEth { get; set; }

        public int Priority { get; set; }

        public string Sender { get; set; }

        public string Recipient { get; set; }

        public string Address { get; set; }

        public string ContractAddress { get; set; }

        public string ContractName { get; set; }

        public string ContractSymbol { get; set; }

        public int ContractDecimals { get; set; }

        public long ContractHolders { get; set; }

        public long ContractIssuances { get; set; }

        public string ContractLink { get; set; }

        public IReadOnlyList<string> Addresses { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DataOperation o &&
                o.Hash.Equals(Hash) &&
                o.Order == Order;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash, Order);
        }
    }
}
