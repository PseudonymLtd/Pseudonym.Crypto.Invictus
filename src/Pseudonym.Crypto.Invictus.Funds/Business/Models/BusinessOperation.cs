using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    public sealed class BusinessOperation : IOperation
    {
        public EthereumTransactionHash Hash { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public decimal Price { get; set; }

        public bool IsEth { get; set; }

        public int Priority { get; set; }

        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public EthereumAddress Address { get; set; }

        public EthereumAddress ContractAddress { get; set; }

        public string ContractName { get; set; }

        public string ContractSymbol { get; set; }

        public int ContractDecimals { get; set; }

        public long ContractHolders { get; set; }

        public long ContractIssuances { get; set; }

        public Uri ContractLink { get; set; }

        public IReadOnlyList<EthereumAddress> Addresses { get; set; }
    }
}
