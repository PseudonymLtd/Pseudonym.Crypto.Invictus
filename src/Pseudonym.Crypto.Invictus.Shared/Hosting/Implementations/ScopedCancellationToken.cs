using System.Threading;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting.Models
{
    internal sealed class ScopedCancellationToken : IScopedCancellationToken
    {
        public CancellationToken Token { get; private set; } = CancellationToken.None;

        public void SetToken(CancellationToken token)
        {
            Token = token;
        }
    }
}
