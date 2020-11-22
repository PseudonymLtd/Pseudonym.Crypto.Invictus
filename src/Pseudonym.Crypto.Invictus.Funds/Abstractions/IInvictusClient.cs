using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IInvictusClient
    {
        IAsyncEnumerable<InvictusFund> ListFundsAsync();

        IAsyncEnumerable<InvictusPerformanceSummary> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to);

        Task<InvictusFund> GetFundAsync(Symbol symbol);
    }
}
