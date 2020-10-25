using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ITransaction
    {
        EthereumAddress Sender { get; }

        EthereumAddress Recipient { get; }

        decimal Amount { get; }
    }
}
