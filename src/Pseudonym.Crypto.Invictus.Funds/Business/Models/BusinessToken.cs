using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessToken : IToken
    {
        public Symbol Symbol { get; set; }

        public int Decimals { get; set; }

        public EthereumAddress ContractAddress { get; set; }
    }
}
