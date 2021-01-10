using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IGraphClient
    {
        Task<UniswapPairResult> GetUniswapPairAsync(EthereumAddress pairAddress);
    }
}
