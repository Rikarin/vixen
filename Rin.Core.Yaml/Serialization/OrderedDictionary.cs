using System.Collections;
using System.Collections.ObjectModel;

namespace Rin.Core.Yaml.Serialization;

public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>,
    IList<KeyValuePair<TKey, TValue>>,
    IDictionary {
    readonly KeyedCollection items = [];

    public int Count => items.Count;
    public bool IsReadOnly => false;
    public ICollection<TKey> Keys => items.Select(x => x.Key).ToList();
    public ICollection<TValue> Values => items.Select(x => x.Value).ToList();
    object ICollection.SyncRoot => ((ICollection)items).SyncRoot;
    bool ICollection.IsSynchronized => ((ICollection)items).IsSynchronized;
    ICollection IDictionary.Keys => (ICollection)Keys;
    ICollection IDictionary.Values => (ICollection)Values;
    bool IDictionary.IsFixedSize => false;

    public KeyValuePair<TKey, TValue> this[int index] {
        get => items[index];
        set => items[index] = value;
    }

    public TValue this[TKey key] {
        get => items[key].Value;
        set {
            var item = new KeyValuePair<TKey, TValue>(key, value);
            var index = IndexOf(key);
            if (index != -1) {
                items[index] = item;
            } else {
                items.Add(item);
            }
        }
    }

    object IDictionary.this[object key] {
        get => this[(TKey)key];
        set => this[(TKey)key] = (TValue)value;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => items.GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item) {
        items.Add(item);
    }

    public void Clear() {
        items.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => items.Contains(item);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        items.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) => items.Remove(item);

    public void Add(TKey key, TValue value) {
        items.Add(new(key, value));
    }

    public bool ContainsKey(TKey key) => items.Contains(key);

    public bool Remove(TKey key) => items.Remove(key);

    public bool TryGetValue(TKey key, out TValue value) {
        if (!items.Contains(key)) {
            value = default;
            return false;
        }

        value = items[key].Value;
        return true;
    }

    public void Insert(int index, TKey key, TValue value) {
        items.Insert(index, new(key, value));
    }

    public void RemoveAt(int index) {
        items.RemoveAt(index);
    }

    public int IndexOf(KeyValuePair<TKey, TValue> item) => items.IndexOf(item);

    public void Insert(int index, KeyValuePair<TKey, TValue> item) {
        items.Insert(index, item);
    }

    public int IndexOf(TKey key) {
        if (!items.Contains(key)) {
            return -1;
        }

        return items.IndexOf(items[key]);
    }

    bool IDictionary.Contains(object key) => ContainsKey((TKey)key);

    void IDictionary.Add(object key, object value) => Add((TKey)key, (TValue)value);

    void IDictionary.Remove(object key) => Remove((TKey)key);

    void ICollection.CopyTo(Array array, int index) => ((ICollection)items).CopyTo(array, index);

    IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    class Enumerator : IDictionaryEnumerator {
        readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;
        public object Current => enumerator.Current;
        public object Key => enumerator.Current.Key;
        public object Value => enumerator.Current.Value;
        public DictionaryEntry Entry => new(Key, Value);

        public Enumerator(OrderedDictionary<TKey, TValue> dictionary) {
            enumerator = dictionary.GetEnumerator();
        }

        public bool MoveNext() => enumerator.MoveNext();
        public void Reset() => enumerator.Reset();
    }

    class KeyedCollection : KeyedCollection<TKey, KeyValuePair<TKey, TValue>> {
        protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item) => item.Key;
    }
}
