using System.Threading;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IScopedCancellationToken
    {
        CancellationToken Token { get; }
    }
}
