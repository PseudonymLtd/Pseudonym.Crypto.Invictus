using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Rpc;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IRpcClient
    {
        Task<ulong> GetCurrentBlockNumberAsync();

        Task<EthereumBlock> GetBlockAsync(ulong blockNumber);

        Task<decimal> GetEthBalanceAsync(EthereumAddress address, ulong? blockNumber = null);

        Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address, int decimals, ulong? blockNumber = null);

        Task<TFunction> GetDataAsync<TFunction>(EthereumAddress contractAddress, string data)
            where TFunction : class, new();
    }
}
