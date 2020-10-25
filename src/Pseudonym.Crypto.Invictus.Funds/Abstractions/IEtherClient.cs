using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IEtherClient
    {
        Task<decimal> GetEthBalanceAsync(EthereumAddress address);

        Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address);

        IAsyncEnumerable<EtherTransaction> ListContractTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address);
    }
}
