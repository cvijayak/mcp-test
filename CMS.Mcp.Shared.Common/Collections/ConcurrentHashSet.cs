namespace CMS.Mcp.Shared.Common.Collections
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class ConcurrentHashSet<T> : IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new();

        public bool Add(T item) => _dictionary.TryAdd(item, 0);

        public bool Remove(T item) => _dictionary.TryRemove(item, out _);

        public bool Contains(T item) => _dictionary.ContainsKey(item);

        public int Count => _dictionary.Count;
        
        public void Clear() => _dictionary.Clear();

        public IEnumerable<T> ToList() => _dictionary.Keys;

        public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
