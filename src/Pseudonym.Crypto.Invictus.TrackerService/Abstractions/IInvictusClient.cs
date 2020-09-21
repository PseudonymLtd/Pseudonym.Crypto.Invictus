using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
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
        IAsyncEnumerable<InvictusFund> ListFundsAsync(CancellationToken cancellationToken);

        IAsyncEnumerable<InvictusPerformance> ListPerformanceAsync(Symbol symbol, DateTime from, DateTime to, CancellationToken cancellationToken);

        Task<InvictusFund> GetFundAsync(Symbol symbol, CancellationToken cancellationToken);
    }
}
