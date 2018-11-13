using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Collections
{
    internal class StringDictionary<T> : IDictionary<string, T>
    {
        private Dictionary<string, T> _dict;

        public StringDictionary(bool ignoreCase)
        {
            var comparer = ignoreCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
            _dict = new Dictionary<string, T>(comparer);
        }

        public T this[string key] { get => _dict[key]; set => _dict[key] = value; }

        public ICollection<string> Keys => _dict.Keys;

        public ICollection<T> Values => _dict.Values;

        public int Count => _dict.Count;

        public bool IsReadOnly => ((IDictionary<string, T>)_dict).IsReadOnly;

        public void Add(string key, T value) => _dict.Add(key, value);

        public void Add(KeyValuePair<string, T> item) => ((IDictionary<string, T>)_dict).Add(item);

        public void Clear() => _dict.Clear();

        public bool Contains(KeyValuePair<string, T> item) => ((IDictionary<string, T>)_dict).Contains(item);

        public bool ContainsKey(string key) => _dict.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) => ((IDictionary<string, T>)_dict).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _dict.GetEnumerator();

        public bool Remove(string key) => _dict.Remove(key);

        public bool Remove(KeyValuePair<string, T> item) => ((IDictionary<string, T>)_dict).Remove(item);

        public bool TryGetValue(string key, out T value) => _dict.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
    }
}
