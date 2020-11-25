using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IOperation
    {
        EthereumTransactionHash Hash { get; }

        int Order { get; }

        string Value { get; }

        decimal PricePerToken { get; }

        decimal Quantity { get; }

        string Type { get; }

        bool IsEth { get; }

        int Priority { get; }

        EthereumAddress? Sender { get; }

        EthereumAddress? Recipient { get; }

        EthereumAddress? Address { get; }

        EthereumAddress ContractAddress { get; }

        string ContractName { get; }

        string ContractSymbol { get; }

        int ContractDecimals { get; }

        long ContractHolders { get; }

        long ContractIssuances { get; }

        Uri ContractLink { get; }

        IReadOnlyList<EthereumAddress> Addresses { get; }
    }
}
