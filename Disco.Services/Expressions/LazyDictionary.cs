using System;
using System.Collections;
using System.Collections.Generic;

namespace Disco.Services.Expressions
{
    public class LazyDictionary : IDictionary<string, string>
    {
        private readonly Lazy<IDictionary<string, string>> dictionary;

        public LazyDictionary(Func<IDictionary<string, string>> dictionaryFactory)
        {
            if (dictionaryFactory == null)
                throw new ArgumentNullException(nameof(dictionaryFactory));

            dictionary = new Lazy<IDictionary<string, string>>(dictionaryFactory);
        }

        public string this[string key]
        {
            get => dictionary.Value.TryGetValue(key, out var value) ? value : null;
            set => throw new NotSupportedException();
        }

        public ICollection<string> Keys => dictionary.Value.Keys;
        public ICollection<string> Values => dictionary.Value.Values;
        public int Count => dictionary.Value.Count;
        public bool IsReadOnly => true;

        public void Add(string key, string value)
            => throw new NotSupportedException();

        public void Add(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        public void Clear()
            => throw new NotSupportedException();

        public bool Contains(KeyValuePair<string, string> item)
            => dictionary.Value.Contains(item);

        public bool ContainsKey(string key)
            => dictionary.Value.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            => throw new NotSupportedException();

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => dictionary.Value.GetEnumerator();

        public bool Remove(string key)
            => throw new NotSupportedException();

        public bool Remove(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        public bool TryGetValue(string key, out string value)
            => dictionary.Value.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => dictionary.Value.GetEnumerator();
    }
}
