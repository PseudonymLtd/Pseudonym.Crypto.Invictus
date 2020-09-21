using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions
{
    public interface IFundService
    {
        IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode, CancellationToken cancellationToken);

        Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode, CancellationToken cancellationToken);

        IAsyncEnumerable<IPerformance> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to, CurrencyCode currencyCode, CancellationToken cancellationToken);
    }
}
