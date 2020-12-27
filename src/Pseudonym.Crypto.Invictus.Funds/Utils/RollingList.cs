using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pseudonym.Crypto.Invictus.Funds.Utils
{
    public class RollingList<T> : IEnumerable<T>
    {
        private readonly LinkedList<T> list;

        public RollingList(int maximumCount)
        {
            if (maximumCount <= 0)
            {
                throw new ArgumentException("Must be greater than 0.", nameof(maximumCount));
            }

            list = new LinkedList<T>();

            MaximumCount = maximumCount;
        }

        public int MaximumCount { get; }

        public int Count => list.Count;

        public T this[int index] => list.Skip(index).First();

        public void Add(T value)
        {
            if (list.Count == MaximumCount)
            {
                list.RemoveFirst();
            }

            list.AddLast(value);
        }

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    }
}
