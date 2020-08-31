using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Investments.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions
{
    public interface IFundService
    {
        IAsyncEnumerable<IFund> ListFundsAsync(CancellationToken cancellationToken);

        Task<IFund> GetFundAsync(string fundName, CancellationToken cancellationToken);
    }
}
