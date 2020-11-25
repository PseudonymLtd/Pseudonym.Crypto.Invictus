using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface ITransactionRepository
    {
        Task<DataTransaction> GetTransactionsAsync(EthereumAddress contractAddress, EthereumTransactionHash hash);

        IAsyncEnumerable<DataTransaction> ListTransactionsAsync(EthereumAddress contractAddress);

        Task UploadTransactionAsync(DataTransaction transaction);

        Task<long> GetLatestBlockNumberAsync(EthereumAddress address);

        Task<long> GetLowestBlockNumberAsync(EthereumAddress address);
    }
}
