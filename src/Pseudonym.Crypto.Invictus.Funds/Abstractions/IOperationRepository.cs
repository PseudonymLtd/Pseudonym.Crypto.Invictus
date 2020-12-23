﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IOperationRepository
    {
        Task<DataOperation> GetOperationAsync(EthereumTransactionHash hash, int order);

        IAsyncEnumerable<DataOperation> ListOperationsAsync(EthereumTransactionHash hash);

        IAsyncEnumerable<EthereumTransactionHash> ListInboundHashesAsync(EthereumAddress address, string type);

        IAsyncEnumerable<EthereumTransactionHash> ListOutboundHashesAsync(EthereumAddress address, string type);

        Task UploadItemsAsync(params DataOperation[] operations);
    }
}
