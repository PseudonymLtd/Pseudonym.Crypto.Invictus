using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> collection, CancellationToken cancellationToken)
        {
            var list = new List<T>();

            await foreach (var transaction in collection.WithCancellation(cancellationToken))
            {
                list.Add(transaction);
            }

            return list;
        }
    }
}
