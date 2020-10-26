using System.Threading;

namespace Pseudonym.Crypto.Invictus.Shared.Abstractions
{
    public interface IScopedCancellationToken
    {
        CancellationToken Token { get; }
    }
}
