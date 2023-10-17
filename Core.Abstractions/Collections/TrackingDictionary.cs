using System.Collections;
using System.Collections.Specialized;

namespace Rin.Core.Collections;

public class TrackingDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ITrackingCollectionChanged {
    readonly Dictionary<TKey, TValue> innerDictionary;

    EventHandler<TrackingCollectionChangedEventArgs> itemAdded;
    EventHandler<TrackingCollectionChangedEventArgs> itemRemoved;

    /// <inheritdoc />
    public ICollection<TKey> Keys => innerDictionary.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => innerDictionary.Values;

    /// <inheritdoc />
    public int Count => innerDictionary.Count;

    /// <inheritdoc />
    public bool IsReadOnly => ((IDictionary<TKey, TValue>)innerDictionary).IsReadOnly;

    /// <inheritdoc />
    bool IDictionary.IsFixedSize => ((IDictionary)innerDictionary).IsFixedSize;

    /// <inheritdoc />
    ICollection IDictionary.Keys => ((IDictionary)innerDictionary).Keys;

    /// <inheritdoc />
    ICollection IDictionary.Values => ((IDictionary)innerDictionary).Values;

    /// <inheritdoc />
    bool ICollection.IsSynchronized => ((IDictionary)innerDictionary).IsSynchronized;

    /// <inheritdoc />
    object ICollection.SyncRoot => ((IDictionary)innerDictionary).SyncRoot;

    /// <inheritdoc />
    public TValue this[TKey key] {
        get => innerDictionary[key];
        set {
            var collectionChangedRemoved = itemRemoved;
            if (collectionChangedRemoved != null) {
                TValue oldValue;

                var alreadyExisting = innerDictionary.TryGetValue(key, out oldValue);
                if (alreadyExisting) {
                    collectionChangedRemoved(
                        this,
                        new(NotifyCollectionChangedAction.Remove, key, oldValue, null, false)
                    );
                }

                innerDictionary[key] = value;

                // Note: CollectionChanged is considered not thread-safe, so no need to skip if null here, shouldn't happen
                itemAdded(
                    this,
                    new(NotifyCollectionChangedAction.Add, key, innerDictionary[key], oldValue, !alreadyExisting)
                );
            } else {
                innerDictionary[key] = value;
            }
        }
    }

    /// <inheritdoc />
    object IDictionary.this[object key] {
        get => this[(TKey)key];
        set => this[(TKey)key] = (TValue)value;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TrackingDictionary{TKey, TValue}" /> class.
    /// </summary>
    public TrackingDictionary() {
        innerDictionary = new();
    }

    /// <inheritdoc />
    public void Add(TKey key, TValue value) {
        innerDictionary.Add(key, value);
        itemAdded?.Invoke(this, new(NotifyCollectionChangedAction.Add, key, value, null, true));
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key) => innerDictionary.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(TKey key) {
        var collectionChanged = itemRemoved;
        if (collectionChanged != null && innerDictionary.TryGetValue(key, out var dictValue)) {
            collectionChanged(this, new(NotifyCollectionChangedAction.Remove, key, dictValue, null, true));
        }

        return innerDictionary.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value) => innerDictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item) {
        Add(item.Key, item.Value);
    }

    /// <inheritdoc />
    public void Clear() {
        var collectionChanged = itemRemoved;
        if (collectionChanged != null) {
            foreach (var key in innerDictionary.Keys.ToArray()) {
                Remove(key);
            }
        } else {
            innerDictionary.Clear();
        }
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item) => innerDictionary.Contains(item);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        ((IDictionary<TKey, TValue>)innerDictionary).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item) {
        var collectionChanged = itemRemoved;
        if (collectionChanged != null && innerDictionary.Contains(item)) {
            return innerDictionary.Remove(item.Key);
        }

        return ((IDictionary<TKey, TValue>)innerDictionary).Remove(item);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => innerDictionary.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => innerDictionary.GetEnumerator();

    /// <inheritdoc />
    void IDictionary.Add(object key, object value) {
        Add((TKey)key, (TValue)value);
    }

    /// <inheritdoc />
    bool IDictionary.Contains(object key) => ((IDictionary)innerDictionary).Contains(key);

    /// <inheritdoc />
    IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)innerDictionary).GetEnumerator();

    /// <inheritdoc />
    void IDictionary.Remove(object key) {
        Remove((TKey)key);
    }

    /// <inheritdoc />
    void ICollection.CopyTo(Array array, int index) {
        ((IDictionary)innerDictionary).CopyTo(array, index);
    }

    /// <inheritdoc />
    public event EventHandler<TrackingCollectionChangedEventArgs> CollectionChanged {
        add {
            // We keep a list in reverse order for removal, so that we can easily have multiple handlers depending on each others
            itemAdded = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Combine(itemAdded, value);
            itemRemoved = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Combine(value, itemRemoved);
        }
        remove {
            itemAdded = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Remove(itemAdded, value);
            itemRemoved = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Remove(itemRemoved, value);
        }
    }
}
