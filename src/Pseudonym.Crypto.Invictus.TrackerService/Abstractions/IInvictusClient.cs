using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IInvictusClient
    {
        IAsyncEnumerable<InvictusFund> ListFundsAsync(CancellationToken cancellationToken);

        Task<InvictusFund> GetFundAsync(string fundName, CancellationToken cancellationToken);
    }
}
