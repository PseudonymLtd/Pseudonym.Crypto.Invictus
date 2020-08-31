using System.Threading;

namespace Pseudonym.Crypto.Invictus.TrackerService.Abstractions
{
    public interface IScopedCancellationToken
    {
        CancellationToken Token { get; }
    }
}
