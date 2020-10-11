using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IFundService
    {
        IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode);

        Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode);

        IAsyncEnumerable<IPerformance> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to, CurrencyCode currencyCode);
    }
}
