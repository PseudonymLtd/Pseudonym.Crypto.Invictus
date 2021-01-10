using System;
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

        Task<IStake> GetStakeAsync(Symbol stakeSymbol, CurrencyCode currencyCode);

        IAsyncEnumerable<IStakingPower> ListStakePowersAsync(
            Symbol stakeSymbol, PriceMode priceMode, DateTime from, DateTime to, CurrencyCode currencyCode);

        IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, DateTime from, DateTime to);

        IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, Symbol fundSymbol, DateTime from, DateTime to);

        IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, EthereumAddress address);

        IAsyncEnumerable<IStakeEvent> ListStakeEventsAsync(Symbol stakeSymbol, EthereumAddress address, Symbol fundSymbol);

        Task<IStakeEvent> GetStakeEventAsync(Symbol stakeSymbol, Symbol fundSymbol, EthereumTransactionHash hash);

        Task<IStakeEvent> GetStakeEventAsync(Symbol stakeSymbol, EthereumAddress address, Symbol fundSymbol, EthereumTransactionHash hash);
    }
}
