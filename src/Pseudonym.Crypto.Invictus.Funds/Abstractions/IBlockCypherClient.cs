using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.BlockCypher;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IBlockCypherClient
    {
        Task<BlockCypherAddressResponse> GetAddressInformationAsync(EthereumAddress contractAddress, long beforeBlock, long afterBlock);
    }
}
