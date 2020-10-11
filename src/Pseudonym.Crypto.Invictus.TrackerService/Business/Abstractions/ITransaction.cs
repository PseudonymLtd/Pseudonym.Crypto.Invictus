using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions
{
    public interface ITransaction
    {
        EthereumAddress Sender { get; }

        EthereumAddress Recipient { get; }

        decimal Amount { get; }
    }
}
