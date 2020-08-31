using System.Threading;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Hosting.Models
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
