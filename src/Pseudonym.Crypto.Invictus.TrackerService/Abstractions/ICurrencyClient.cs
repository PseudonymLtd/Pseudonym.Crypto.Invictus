using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface ICurrencyClient
    {
        Task<CurrencyRates> GetRatesAsync(CancellationToken cancellationToken);
    }
}
