using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessOperation : IOperation
    {
        public EthereumTransactionHash Hash { get; set; }

        public int Order { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public decimal PricePerToken { get; set; }

        public decimal Quantity { get; set; }

        public bool IsEth { get; set; }

        public int Priority { get; set; }

        public EthereumAddress? Sender { get; set; }

        public EthereumAddress? Recipient { get; set; }

        public EthereumAddress? Address { get; set; }

        public EthereumAddress ContractAddress { get; set; }

        public string ContractName { get; set; }

        public string ContractSymbol { get; set; }

        public int ContractDecimals { get; set; }

        public long ContractHolders { get; set; }

        public long ContractIssuances { get; set; }

        public Uri ContractLink { get; set; }

        public Uri ContractImageLink { get; set; }

        public Uri ContractMarketLink { get; set; }

        public IReadOnlyList<EthereumAddress> Addresses { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BusinessOperation o &&
                o.Hash.Equals(Hash) &&
                o.Order == Order;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash, Order);
        }
    }
}
