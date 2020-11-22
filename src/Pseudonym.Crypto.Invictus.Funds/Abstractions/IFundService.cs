using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IFundService
    {
        IAsyncEnumerable<IFund> ListFundsAsync(CurrencyCode currencyCode);

        Task<IFund> GetFundAsync(Symbol symbol, CurrencyCode currencyCode);

        IAsyncEnumerable<IPerformance> ListPerformanceAsync(Symbol symbol, PriceMode priceMode, DateTime from, DateTime to, CurrencyCode currencyCode);
    }
}
