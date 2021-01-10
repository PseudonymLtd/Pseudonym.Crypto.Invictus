using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IApiClient
    {
        IAsyncEnumerable<ApiInvestment> ListInvestmentsAsync();

        Task<ApiInvestment> GetInvestmentAsync(Symbol symbol);

        IAsyncEnumerable<ApiTransactionSet> ListInvestmentTransactionsAsync(Symbol symbol);

        Task<ApiTransactionSet> GetInvestmentTransactionAsync(Symbol symbol, string hash);

        IAsyncEnumerable<ApiFund> ListFundsAsync();

        Task<ApiFund> GetFundAsync(Symbol symbol);

        IAsyncEnumerable<ApiPerformance> ListFundPerformanceAsync(Symbol symbol, PriceMode mode, DateTime fromDate, DateTime toDate);

        IAsyncEnumerable<ApiTransactionSet> ListFundTransactionsAsync(Symbol symbol);

        Task<ApiTransactionSet> GetFundTransactionAsync(Symbol symbol, string hash);

        IAsyncEnumerable<ApiTransactionSet> ListFundBurnsAsync(Symbol symbol);

        IAsyncEnumerable<ApiStake> ListStakesAsync();

        Task<ApiStake> GetStakeAsync(Symbol symbol);

        IAsyncEnumerable<ApiStakingPower> ListStakePowerPerformanceAsync(Symbol symbol, PriceMode mode, DateTime fromDate, DateTime toDate);

        IAsyncEnumerable<ApiStakeEvent> ListStakeEventsAsync(Symbol symbol);

        IAsyncEnumerable<ApiStakeEvent> ListStakeEventsAsync(Symbol symbol, Symbol fundSymbol);

        Task<ApiStakeEvent> GetStakeEventAsync(Symbol symbol, Symbol fundSymbol, string hash);
    }
}
