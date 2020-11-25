using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IAddressService
    {
        IAsyncEnumerable<IInvestment> ListInvestmentsAsync(EthereumAddress address, CurrencyCode currencyCode);

        Task<IInvestment> GetInvestmentAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode);

        IAsyncEnumerable<ITransactionSet> ListTransactionsAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode);
    }
}
