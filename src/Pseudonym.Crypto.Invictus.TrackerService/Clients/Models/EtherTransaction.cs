using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Clients.Models
{
    public sealed class EtherTransaction
    {
        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public decimal Amount { get; set; }
    }
}
