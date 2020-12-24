using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IStakeService
    {
        IAsyncEnumerable<IStake> ListStakesAsync(CurrencyCode currencyCode);

        IAsyncEnumerable<IStake> ListStakesAsync(EthereumAddress address, CurrencyCode currencyCode);

        IAsyncEnumerable<IStake> ListStakesAsync(Symbol symbol, CurrencyCode currencyCode);

        IAsyncEnumerable<IStake> ListStakesAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode);

        Task<IStake> GetStakeAsync(Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode);

        Task<IStake> GetStakeAsync(EthereumAddress address, Symbol symbol, EthereumTransactionHash hash, CurrencyCode currencyCode);
    }
}
