using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessToken : IToken
    {
        public Symbol Symbol { get; set; }

        public int Decimals { get; set; }

        public EthereumAddress ContractAddress { get; set; }
    }
}
