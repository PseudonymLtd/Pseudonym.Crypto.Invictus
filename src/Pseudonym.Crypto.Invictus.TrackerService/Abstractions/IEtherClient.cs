using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IEtherClient
    {
        Task<decimal> GetEthBalanceAsync(EthereumAddress address);

        Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address);

        IAsyncEnumerable<EtherTransaction> ListContractTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address);
    }
}
