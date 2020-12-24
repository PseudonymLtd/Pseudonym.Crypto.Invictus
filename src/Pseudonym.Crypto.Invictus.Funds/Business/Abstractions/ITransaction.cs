using System;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ITransaction
    {
        EthereumAddress Address { get; }

        EthereumTransactionHash Hash { get; }

        long Confirmations { get; }

        long BlockNumber { get; }

        DateTime ConfirmedAt { get; }

        EthereumAddress Sender { get; }

        EthereumAddress Recipient { get; }

        string Input { get; }

        long GasLimit { get; }

        long Gas { get; }

        decimal GasUsed { get; }

        decimal Eth { get; }

        bool Success { get; }
    }
}
