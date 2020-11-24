using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Funds.Utils;

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

        long GasLimit { get; }

        long GasUsed { get; }

        decimal EthValue { get; }

        bool Success { get; }

        string Input { get; }

        [DynamoDbIgnore]
        IReadOnlyList<IOperation> Operations { get; }
    }
}
