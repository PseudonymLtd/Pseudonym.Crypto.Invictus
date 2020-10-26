using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface IToken
    {
        Symbol Symbol { get; }

        int Decimals { get; }

        EthereumAddress ContractAddress { get; }
    }
}
