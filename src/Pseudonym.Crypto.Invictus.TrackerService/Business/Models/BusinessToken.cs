using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Models
{
    internal sealed class BusinessToken : IToken
    {
        public Symbol Symbol { get; set; }

        public int Decimals { get; set; }

        public EthereumAddress ContractAddress { get; set; }
    }
}
