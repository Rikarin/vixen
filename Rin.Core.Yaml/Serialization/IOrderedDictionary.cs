namespace Rin.Core.Yaml.Serialization;

public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
    KeyValuePair<TKey, TValue> this[int index] { get; set; }
    void Insert(int index, TKey key, TValue value);
    void RemoveAt(int index);
    int IndexOf(TKey key);
}
