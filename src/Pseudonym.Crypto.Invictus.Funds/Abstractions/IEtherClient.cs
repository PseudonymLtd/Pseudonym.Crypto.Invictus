using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IEtherClient
    {
        Task<long> GetCurrentBlockNumberAsync();

        Task<decimal> GetEthBalanceAsync(EthereumAddress address);

        Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address);

        Task<TransactionReceipt> GetTransactionAsync(EthereumTransactionHash hash);
    }
}
