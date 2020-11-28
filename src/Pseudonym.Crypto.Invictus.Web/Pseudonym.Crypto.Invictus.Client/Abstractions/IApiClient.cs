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

        IAsyncEnumerable<ApiFund> ListFundsAsync();

        Task<ApiFund> GetFundAsync(Symbol symbol);

        IAsyncEnumerable<ApiPerformance> ListFundPerformanceAsync(Symbol symbol, PriceMode marketMode, DateTime fromDate, DateTime toDate);
    }
}
