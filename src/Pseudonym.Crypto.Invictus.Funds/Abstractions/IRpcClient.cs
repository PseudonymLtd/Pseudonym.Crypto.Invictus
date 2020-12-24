using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IRpcClient
    {
        Task<long> GetCurrentBlockNumberAsync();

        Task<decimal> GetEthBalanceAsync(EthereumAddress address);

        Task<decimal> GetContractBalanceAsync(EthereumAddress contractAddress, EthereumAddress address, int decimals);

        Task<TFunction> GetDataAsync<TFunction>(EthereumAddress contractAddress, string data)
            where TFunction : class, new();
    }
}
