using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IAddressService
    {
        IAsyncEnumerable<IInvestment> ListInvestmentsAsync(EthereumAddress address, CurrencyCode currencyCode);

        IAsyncEnumerable<ITransaction> ListTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address, CurrencyCode currencyCode);
    }
}
