using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public abstract class ApiCollection<TValue> : IDictionary<string, TValue>
    {
        [JsonIgnore]
        public ICollection<string> Keys => Items.Keys;

        [JsonIgnore]
        public ICollection<TValue> Values => Items.Values;

        [JsonIgnore]
        public int Count => Items.Count;

        [JsonIgnore]
        public bool IsReadOnly => Items.IsReadOnly;

        [JsonIgnore]
        protected IDictionary<string, TValue> Items { get; set; } = new Dictionary<string, TValue>();

        [JsonIgnore]
        public TValue this[string key]
        {
            get
            {
                var keyLower = key.ToLower();

                return Items.ContainsKey(keyLower)
                    ? Items[keyLower]
                    : default;
            }
            set => Add(key, value);
        }

        public void Add(string key, TValue value)
        {
            var keyLower = key.ToLower();

            if (Items.ContainsKey(keyLower))
            {
                Items[keyLower] = value;
            }
            else
            {
                Items.Add(keyLower, value);
            }
        }

        public bool ContainsKey(string key)
        {
            return Items.ContainsKey(key.ToLower());
        }

        public bool Remove(string key)
        {
            return Items.Remove(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            value = this[key];

            return value != null;
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return Items.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
