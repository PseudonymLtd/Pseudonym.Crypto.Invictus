using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Investments.Business.Abstractions
{
    public interface IToken
    {
        Symbol Symbol { get; }

        int Decimals { get; }

        EthereumAddress ContractAddress { get; }
    }
}
