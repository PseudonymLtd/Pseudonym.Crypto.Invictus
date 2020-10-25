using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessTransaction : ITransaction
    {
        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public decimal Amount { get; set; }
    }
}
