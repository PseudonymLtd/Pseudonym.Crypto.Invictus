using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface ITransactionRepository : IRepository<DataTransaction>
    {
        Task<DataTransaction> GetTransactionAsync(EthereumAddress contractAddress, EthereumTransactionHash hash);

        IAsyncEnumerable<DataTransaction> ListTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumTransactionHash? startHash,
            DateTime? offset,
            DateTime from,
            DateTime to);

        IAsyncEnumerable<DataTransaction> ListInboundTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumAddress address,
            EthereumAddress? filterAddress = null,
            DateTime? from = null,
            DateTime? to = null);

        IAsyncEnumerable<DataTransaction> ListOutboundTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumAddress address,
            EthereumAddress? filterAddress = null,
            DateTime? from = null,
            DateTime? to = null);

        Task<long> GetLatestBlockNumberAsync(EthereumAddress address);

        Task<long> GetLowestBlockNumberAsync(EthereumAddress address);

        Task<DateTime?> GetLatestDateAsync(EthereumAddress address);

        Task<DateTime?> GetLowestDateAsync(EthereumAddress address);
    }
}
