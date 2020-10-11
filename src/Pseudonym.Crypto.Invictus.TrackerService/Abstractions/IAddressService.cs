using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Ethereum;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IAddressService
    {
        IAsyncEnumerable<IInvestment> ListInvestmentsAsync(EthereumAddress address, CurrencyCode currencyCode);

        IAsyncEnumerable<ITransaction> ListTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address, CurrencyCode currencyCode);
    }
}
