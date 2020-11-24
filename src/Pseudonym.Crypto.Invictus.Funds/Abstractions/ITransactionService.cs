using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface ITransactionService
    {
        IReadOnlyList<ITransaction> GetAddressTransactions(EthereumAddress contractAddress, EthereumAddress address);

        Task UploadTransactionAsync(ITransaction transaction);

        Task<long> GetLastBlockNumberAsync(EthereumAddress address);
    }
}
