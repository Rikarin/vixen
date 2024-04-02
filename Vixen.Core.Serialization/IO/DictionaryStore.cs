namespace Vixen.Core.Serialization.IO;

/// <summary>
///     A Key->Value store that will be incrementally saved on the HDD.
///     Thread-safe and process-safe.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class DictionaryStore<TKey, TValue> : Store<KeyValuePair<TKey, TValue>> {
    protected readonly Dictionary<TKey, TValue> loadedIdMap = new();
    protected readonly Dictionary<TKey, UnsavedIdMapEntry> unsavedIdMap = new();

    /// <summary>
    ///     Gets or sets the item with the specified key.
    /// </summary>
    /// <value>
    ///     The item to get or set.
    /// </value>
    /// <param name="key">The key of the item to get or set.</param>
    /// <returns></returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
    public TValue this[TKey key] {
        get {
            if (!TryGetValue(key, out var value)) {
                throw new KeyNotFoundException();
            }

            return value;
        }
        set => AddValue(new(key, value));
    }

    public DictionaryStore(Stream stream) : base(stream) { }

    /// <summary>
    ///     Gets the values stored including unsaved.
    /// </summary>
    /// <returns>Values stored including unsaved.</returns>
    public KeyValuePair<TKey, TValue>[] GetValues() {
        lock (lockObject) {
            var result = new KeyValuePair<TKey, TValue>[loadedIdMap.Count + unsavedIdMap.Count];
            var i = 0;
            foreach (var value in loadedIdMap) {
                result[i++] = value;
            }

            foreach (var item in unsavedIdMap) {
                result[i++] = new(item.Key, item.Value.Value);
            }

            return result;
        }
    }

    /// <summary>
    ///     Gets or sets the item with the specified key.
    /// </summary>
    /// <value>
    ///     The item to get or set.
    /// </value>
    /// <param name="key">The key of the item to get or set.</param>
    /// <returns></returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
    public bool Contains(TKey key) => TryGetValue(key, out _);

    /// <summary>
    ///     Tries to get the value from its key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool TryGetValue(TKey key, out TValue value) {
        lock (lockObject) {
            if (unsavedIdMap.TryGetValue(key, out var unsavedIdMapEntry)) {
                value = unsavedIdMapEntry.Value;
                return true;
            }

            return loadedIdMap.TryGetValue(key, out value);
        }
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> SearchValues(Func<KeyValuePair<TKey, TValue>, bool> predicate) {
        lock (lockObject) {
            var result = new Dictionary<TKey, TValue>(loadedIdMap.Count + unsavedIdMap.Count);

            foreach (var item in loadedIdMap) {
                if (predicate(new(item.Key, item.Value))) {
                    result[item.Key] = item.Value;
                }
            }

            foreach (var item in unsavedIdMap) {
                if (predicate(new(item.Key, item.Value.Value))) {
                    result[item.Key] = item.Value.Value;
                }
            }

            return result;
        }
    }

    protected override void AddUnsaved(KeyValuePair<TKey, TValue> item, int currentTransaction) {
        var unsavedIdMapEntry = new UnsavedIdMapEntry { Value = item.Value, Transaction = currentTransaction };
        unsavedIdMap[item.Key] = unsavedIdMapEntry;
    }

    protected override void RemoveUnsaved(KeyValuePair<TKey, TValue> item, int currentTransaction) {
        if (unsavedIdMap.TryGetValue(item.Key, out var entry)) {
            if (entry.Transaction == currentTransaction) {
                unsavedIdMap.Remove(item.Key);
            }
        }
    }

    protected override void AddLoaded(KeyValuePair<TKey, TValue> item) {
        loadedIdMap[item.Key] = item.Value;
    }

    protected override IEnumerable<KeyValuePair<TKey, TValue>> GetPendingItems(int currentTransaction) {
        var transactionIds = new List<KeyValuePair<TKey, TValue>>();

        foreach (var unsavedIdMapEntry in unsavedIdMap.Where(x => x.Value.Transaction == currentTransaction)) {
            transactionIds.Add(new(unsavedIdMapEntry.Key, unsavedIdMapEntry.Value.Value));
        }

        return transactionIds.ToArray();
    }

    protected override void ResetInternal() {
        loadedIdMap.Clear();
        unsavedIdMap.Clear();
    }

    protected struct UnsavedIdMapEntry {
        public int Transaction;
        public TValue Value;
    }
}
