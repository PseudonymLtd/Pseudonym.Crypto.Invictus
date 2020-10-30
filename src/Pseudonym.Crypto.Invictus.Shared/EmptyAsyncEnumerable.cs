using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pseudonym.Crypto.Invictus.Shared
{
    public sealed class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return EnumerateAsync().GetAsyncEnumerator();
        }

        private async IAsyncEnumerable<T> EnumerateAsync()
        {
            await Task.CompletedTask;

            foreach (var item in Enumerable.Empty<T>())
            {
                yield return item;
            }
        }
    }
}
