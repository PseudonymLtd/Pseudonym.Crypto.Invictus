using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions
{
    public interface IToken
    {
        Symbol Symbol { get; }

        int Decimals { get; }

        EthereumAddress ContractAddress { get; }
    }
}
