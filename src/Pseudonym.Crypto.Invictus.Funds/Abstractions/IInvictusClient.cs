using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public enum Symbol
    {
        C20,
        C10,
        IHF,
        IML,
        EMS,
        IGP,
        IBA
    }

    public interface IInvictusClient
    {
        IAsyncEnumerable<InvictusFund> ListFundsAsync();

        IAsyncEnumerable<InvictusPerformance> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to);

        Task<InvictusFund> GetFundAsync(Symbol symbol);
    }
}
