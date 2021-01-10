using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Ethplorer;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IEthplorerClient
    {
        Task<EthplorerTokenInfo> GetTokenInfoAsync(EthereumAddress contractAddress);

        Task<EthplorerPriceData> GetTokenPricingAsync(EthereumAddress contractAddress);

        Task<EthplorerTransaction> GetTransactionAsync(EthereumTransactionHash hash);
    }
}
