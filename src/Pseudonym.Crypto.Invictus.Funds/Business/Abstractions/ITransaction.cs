using System;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ITransaction
    {
        EthereumAddress Address { get; }

        EthereumTransactionHash Hash { get; }

        EthereumTransactionHash BlockHash { get; }

        long Nonce { get; }

        long Confirmations { get; }

        long BlockNumber { get; }

        DateTime ConfirmedAt { get; }

        EthereumAddress Sender { get; }

        EthereumAddress Recipient { get; }

        long GasLimit { get; }

        long Gas { get; }

        decimal GasPrice { get; set; }

        decimal GasUsed { get; }

        decimal Eth { get; }

        bool Success { get; }

        string Input { get; }
    }
}
