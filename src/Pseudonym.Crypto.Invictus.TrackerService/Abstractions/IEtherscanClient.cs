using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IEtherscanClient
    {
        Task<decimal> GetTokensForAddressAsync(IToken token, EthereumAddress etherAddress, CancellationToken cancellationToken);
    }
}
