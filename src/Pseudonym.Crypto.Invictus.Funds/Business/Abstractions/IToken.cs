using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IToken
    {
        Symbol Symbol { get; }

        int Decimals { get; }

        EthereumAddress ContractAddress { get; }
    }
}
