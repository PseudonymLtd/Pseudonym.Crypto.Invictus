using System;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IEtherClient : IDisposable
    {
        Task<decimal> GetEthBalance(EthereumAddress address);

        Task<decimal> GetContractBalance(EthereumAddress contractAddress, EthereumAddress address);
    }
}
