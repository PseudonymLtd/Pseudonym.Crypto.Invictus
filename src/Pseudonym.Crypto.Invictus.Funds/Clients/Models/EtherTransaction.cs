using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models
{
    public sealed class EtherTransaction
    {
        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public decimal Amount { get; set; }
    }
}
