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

        IAsyncEnumerable<InvictusPerformance> ListPerformanceAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to);

        Task<InvictusFund> GetFundAsync(Symbol symbol);
    }
}
