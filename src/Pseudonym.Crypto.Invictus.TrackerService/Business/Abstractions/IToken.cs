using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Investments.Business.Abstractions
{
    public interface IToken
    {
        string Symbol { get; }

        int Decimals { get; }

        EthereumAddress ContractAddress { get; }
    }
}
