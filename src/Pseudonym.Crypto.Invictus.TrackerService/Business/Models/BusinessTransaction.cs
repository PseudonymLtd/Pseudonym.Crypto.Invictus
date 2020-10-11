using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Models
{
    internal sealed class BusinessTransaction : ITransaction
    {
        public EthereumAddress Sender { get; set; }

        public EthereumAddress Recipient { get; set; }

        public decimal Amount { get; set; }
    }
}
