using System.Threading;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface ICurrencyClient
    {
        Task<CurrencyRates> GetRatesAsync(CancellationToken cancellationToken);
    }
}
