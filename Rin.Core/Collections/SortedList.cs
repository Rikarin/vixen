using System.Collections;
using System.Diagnostics;

namespace Rin.Core.Collections;

/// <summary>
///     Represents a collection of associated keys and values
///     that are sorted by the keys and are accessible by key
///     and by index.
/// </summary>
[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary {
    static readonly int INITIAL_SIZE = 16;

    int modificationCount;
    KeyValuePair<TKey, TValue>[] table;
    int defaultCapacity;

    public int Count { get; private set; }

    public int Capacity {
        get => table.Length;

        set {
            var current = table.Length;

            if (Count > value) {
                throw new ArgumentOutOfRangeException("capacity too small");
            }

            if (value == 0) {
                // return to default size
                var newTable = new KeyValuePair<TKey, TValue>[defaultCapacity];
                Array.Copy(table, newTable, Count);
                table = newTable;
            } else if (value > Count) {
                var newTable = new KeyValuePair<TKey, TValue>[value];
                Array.Copy(table, newTable, Count);
                table = newTable;
            } else if (value > current) {
                var newTable = new KeyValuePair<TKey, TValue>[value];
                Array.Copy(table, newTable, current);
                table = newTable;
            }
        }
    }

    public IList<TKey> Keys => new ListKeys(this);
    public IList<TValue> Values => new ListValues(this);
    public IComparer<TKey> Comparer { get; private set; }
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    // IDictionary

    bool IDictionary.IsFixedSize => false;
    bool IDictionary.IsReadOnly => false;
    ICollection IDictionary.Keys => new ListKeys(this);
    ICollection IDictionary.Values => new ListValues(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public TValue this[TKey key] {
        get {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            var i = Find(key);

            if (i >= 0) {
                return table[i].Value;
            }

            throw new KeyNotFoundException();
        }
        set {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            PutImpl(key, value, true);
        }
    }

    object IDictionary.this[object key] {
        get {
            if (key is not TKey key1) {
                return null;
            }

            return this[key1];
        }

        set => this[ToKey(key)] = ToValue(value);
    }

    //
    // Constructors
    //
    public SortedList()
        : this(INITIAL_SIZE, null) { }

    public SortedList(int capacity)
        : this(capacity, null) { }

    public SortedList(int capacity, IComparer<TKey> comparer) {
        if (capacity < 0) {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        defaultCapacity = capacity == 0 ? 0 : INITIAL_SIZE;
        Init(comparer, capacity, true);
    }

    public SortedList(IComparer<TKey> comparer)
        : this(INITIAL_SIZE, comparer) { }

    public SortedList(IDictionary<TKey, TValue> dictionary)
        : this(dictionary, null) { }

    public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer) {
        if (dictionary == null) {
            throw new ArgumentNullException(nameof(dictionary));
        }

        Init(comparer, dictionary.Count, true);

        foreach (var kvp in dictionary) {
            Add(kvp.Key, kvp.Value);
        }
    }

    //
    // Public instance methods.
    //

    public void Add(TKey key, TValue value) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        PutImpl(key, value, false);
    }

    public bool ContainsKey(TKey key) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        return Find(key) >= 0;
    }

    public bool Remove(TKey key) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        var i = IndexOfKey(key);
        if (i >= 0) {
            RemoveAt(i);
            return true;
        }

        return false;
    }

    public void Clear() {
        defaultCapacity = INITIAL_SIZE;
        table = new KeyValuePair<TKey, TValue>[defaultCapacity];
        Count = 0;
        modificationCount++;
    }

    // IEnumerable<KeyValuePair<TKey, TValue>>

    public Enumerator GetEnumerator() => new(this);

    //
    // SortedList<TKey, TValue>
    //

    public void RemoveAt(int index) {
        var table = this.table;
        var cnt = Count;
        if (index >= 0 && index < cnt) {
            if (index != cnt - 1) {
                Array.Copy(table, index + 1, table, index, cnt - 1 - index);
            } else {
                table[index] = default;
            }

            --Count;
            ++modificationCount;
        } else {
            throw new ArgumentOutOfRangeException("index out of range");
        }
    }

    public int IndexOfKey(TKey key) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        var indx = 0;
        try {
            indx = Find(key);
        } catch (Exception) {
            throw new InvalidOperationException();
        }

        return indx | (indx >> 31);
    }

    public int IndexOfValue(TValue value) {
        if (Count == 0) {
            return -1;
        }

        for (var i = 0; i < Count; i++) {
            var current = table[i];

            if (Equals(value, current.Value)) {
                return i;
            }
        }

        return -1;
    }

    public bool ContainsValue(TValue value) => IndexOfValue(value) >= 0;

    public void TrimExcess() {
        if (Count < table.Length * 0.9) {
            Capacity = Count;
        }
    }

    public bool TryGetValue(TKey key, out TValue value) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        var i = Find(key);

        if (i >= 0) {
            value = table[i].Value;
            return true;
        }

        value = default;
        return false;
    }

    // ICollection<KeyValuePair<TKey, TValue>>

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
        defaultCapacity = INITIAL_SIZE;
        table = new KeyValuePair<TKey, TValue>[defaultCapacity];
        Count = 0;
        modificationCount++;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        if (Count == 0) {
            return;
        }

        if (null == array) {
            throw new ArgumentNullException();
        }

        if (arrayIndex < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (arrayIndex >= array.Length) {
            throw new ArgumentNullException("arrayIndex is greater than or equal to array.Length");
        }

        if (Count > array.Length - arrayIndex) {
            throw new ArgumentNullException("Not enough space in array from arrayIndex to end of array");
        }

        var i = arrayIndex;
        foreach (var pair in this) {
            array[i++] = pair;
        }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) {
        Add(keyValuePair.Key, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair) {
        var i = Find(keyValuePair.Key);

        if (i >= 0) {
            return Comparer<KeyValuePair<TKey, TValue>>.Default.Compare(table[i], keyValuePair) == 0;
        }

        return false;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) {
        var i = Find(keyValuePair.Key);

        if (i >= 0 && Comparer<KeyValuePair<TKey, TValue>>.Default.Compare(table[i], keyValuePair) == 0) {
            RemoveAt(i);
            return true;
        }

        return false;
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
        new Enumerator(this);

    // IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // IDictionary

    void IDictionary.Add(object key, object value) {
        PutImpl(ToKey(key), ToValue(value), false);
    }

    bool IDictionary.Contains(object key) {
        if (null == key) {
            throw new ArgumentNullException();
        }

        if (!(key is TKey)) {
            return false;
        }

        return Find((TKey)key) >= 0;
    }

    IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(this, EnumeratorMode.ENTRY_MODE);

    void IDictionary.Remove(object key) {
        if (null == key) {
            throw new ArgumentNullException(nameof(key));
        }

        if (!(key is TKey)) {
            return;
        }

        var i = IndexOfKey((TKey)key);
        if (i >= 0) {
            RemoveAt(i);
        }
    }

    // ICollection

    void ICollection.CopyTo(Array array, int arrayIndex) {
        if (Count == 0) {
            return;
        }

        if (null == array) {
            throw new ArgumentNullException();
        }

        if (arrayIndex < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (array.Rank > 1) {
            throw new ArgumentException("array is multi-dimensional");
        }

        if (arrayIndex >= array.Length) {
            throw new ArgumentNullException("arrayIndex is greater than or equal to array.Length");
        }

        if (Count > array.Length - arrayIndex) {
            throw new ArgumentNullException("Not enough space in array from arrayIndex to end of array");
        }

        IEnumerator<KeyValuePair<TKey, TValue>> it = GetEnumerator();
        var i = arrayIndex;

        while (it.MoveNext()) {
            array.SetValue(it.Current, i++);
        }
    }

    //
    // Private methods
    //

    void EnsureCapacity(int n, int free) {
        var table = this.table;
        KeyValuePair<TKey, TValue>[] newTable = null;
        var cap = Capacity;
        var gap = free >= 0 && free < Count;

        if (n > cap) {
            newTable = new KeyValuePair<TKey, TValue>[n << 1];
        }

        if (newTable != null) {
            if (gap) {
                var copyLen = free;
                if (copyLen > 0) {
                    Array.Copy(table, 0, newTable, 0, copyLen);
                }

                copyLen = Count - free;
                if (copyLen > 0) {
                    Array.Copy(table, free, newTable, free + 1, copyLen);
                }
            } else {
                // Just a resizing, copy the entire table.
                Array.Copy(table, newTable, Count);
            }

            this.table = newTable;
        } else if (gap) {
            Array.Copy(table, free, table, free + 1, Count - free);
        }
    }

    void PutImpl(TKey key, TValue value, bool overwrite) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        var table = this.table;

        var freeIndx = -1;

        try {
            freeIndx = Find(key);
        } catch (Exception) {
            throw new InvalidOperationException();
        }

        if (freeIndx >= 0) {
            if (!overwrite) {
                throw new ArgumentException("element already exists");
            }

            table[freeIndx] = new(key, value);
            ++modificationCount;
            return;
        }

        freeIndx = ~freeIndx;

        if (freeIndx > Capacity + 1) {
            throw new("SortedList::internal error (" + key + ", " + value + ") at [" + freeIndx + "]");
        }


        EnsureCapacity(Count + 1, freeIndx);

        table = this.table;
        table[freeIndx] = new(key, value);

        ++Count;
        ++modificationCount;
    }

    void Init(IComparer<TKey> comparer, int capacity, bool forceSize) {
        if (comparer == null) {
            comparer = Comparer<TKey>.Default;
        }

        Comparer = comparer;
        if (!forceSize && capacity < defaultCapacity) {
            capacity = defaultCapacity;
        }

        table = new KeyValuePair<TKey, TValue>[capacity];
        Count = 0;
        modificationCount = 0;
    }

    void CopyToArray(
        Array arr,
        int i,
        EnumeratorMode mode
    ) {
        if (arr == null) {
            throw new ArgumentNullException(nameof(arr));
        }

        if (i < 0 || i + Count > arr.Length) {
            throw new ArgumentOutOfRangeException(nameof(i));
        }

        IEnumerator it = new DictionaryEnumerator(this, mode);

        while (it.MoveNext()) {
            arr.SetValue(it.Current, i++);
        }
    }

    int Find(TKey key) {
        var table = this.table;
        var len = Count;

        if (len == 0) {
            return ~0;
        }

        var left = 0;
        var right = len - 1;

        while (left <= right) {
            var guess = (left + right) >> 1;

            var cmp = Comparer.Compare(table[guess].Key, key);
            if (cmp == 0) {
                return guess;
            }

            if (cmp < 0) {
                left = guess + 1;
            } else {
                right = guess - 1;
            }
        }

        return ~left;
    }

    TKey ToKey(object key) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key));
        }

        if (!(key is TKey)) {
            throw new ArgumentException(
                "The value \""
                + key
                + "\" isn't of type \""
                + typeof(TKey)
                + "\" and can't be used in this generic collection.",
                nameof(key)
            );
        }

        return (TKey)key;
    }

    TValue ToValue(object value) {
        if (!(value is TValue)) {
            throw new ArgumentException(
                "The value \""
                + value
                + "\" isn't of type \""
                + typeof(TValue)
                + "\" and can't be used in this generic collection.",
                nameof(value)
            );
        }

        return (TValue)value;
    }

    enum EnumeratorMode {
        KEY_MODE = 0,
        VALUE_MODE,
        ENTRY_MODE
    }

    internal TKey KeyAt(int index) {
        if (index >= 0 && index < Count) {
            return table[index].Key;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    internal TValue ValueAt(int index) {
        if (index >= 0 && index < Count) {
            return table[index].Value;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    //
    // Inner classes
    //

    public sealed class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
        SortedList<TKey, TValue> host;
        int pos = -1;

        public KeyValuePair<TKey, TValue> Current => host.table[pos];
        object IEnumerator.Current => Current;

        public Enumerator(SortedList<TKey, TValue> host) {
            this.host = host;
        }

        public void Dispose() {
            host = null;
        }

        public bool MoveNext() => ++pos < host.Count;

        public void Reset() {
            throw new NotSupportedException();
        }
    }


    sealed class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator {
        readonly SortedList<TKey, TValue> host;
        int stamp;
        int pos;
        int size;
        readonly EnumeratorMode mode;

        object currentKey;
        object currentValue;

        bool invalid;

        static readonly string xstr = "SortedList.Enumerator: snapshot out of sync.";

        public DictionaryEntry Entry {
            get {
                if (invalid || pos >= size || pos == -1) {
                    throw new InvalidOperationException(xstr);
                }

                return new(
                    currentKey,
                    currentValue
                );
            }
        }

        public object Key {
            get {
                if (invalid || pos >= size || pos == -1) {
                    throw new InvalidOperationException(xstr);
                }

                return currentKey;
            }
        }

        public object Value {
            get {
                if (invalid || pos >= size || pos == -1) {
                    throw new InvalidOperationException(xstr);
                }

                return currentValue;
            }
        }

        public object Current {
            get {
                if (invalid || pos >= size || pos == -1) {
                    throw new InvalidOperationException(xstr);
                }

                switch (mode) {
                    case EnumeratorMode.KEY_MODE:
                        return currentKey;
                    case EnumeratorMode.VALUE_MODE:
                        return currentValue;
                    case EnumeratorMode.ENTRY_MODE:
                        return Entry;

                    default:
                        throw new NotSupportedException(mode + " is not a supported mode.");
                }
            }
        }

        public DictionaryEnumerator(SortedList<TKey, TValue> host, EnumeratorMode mode) {
            this.host = host;
            stamp = host.modificationCount;
            size = host.Count;
            this.mode = mode;
            Reset();
        }

        public DictionaryEnumerator(SortedList<TKey, TValue> host)
            : this(host, EnumeratorMode.ENTRY_MODE) { }

        public void Reset() {
            if (host.modificationCount != stamp || invalid) {
                throw new InvalidOperationException(xstr);
            }

            pos = -1;
            currentKey = null;
            currentValue = null;
        }

        public bool MoveNext() {
            if (host.modificationCount != stamp || invalid) {
                throw new InvalidOperationException(xstr);
            }

            var table = host.table;

            if (++pos < size) {
                var entry = table[pos];

                currentKey = entry.Key;
                currentValue = entry.Value;
                return true;
            }

            currentKey = null;
            currentValue = null;
            return false;
        }

        // ICloneable

        public object Clone() =>
            new DictionaryEnumerator(host, mode) {
                stamp = stamp,
                pos = pos,
                size = size,
                currentKey = currentKey,
                currentValue = currentValue,
                invalid = invalid
            };
    }

    struct KeyEnumerator : IEnumerator<TKey>, IDisposable {
        const int NOT_STARTED = -2;

        // this MUST be -1, because we depend on it in move next.
        // we just decr the size, so, 0 - 1 == FINISHED
        const int FINISHED = -1;

        readonly SortedList<TKey, TValue> l;
        int idx;
        readonly int ver;

        public TKey Current {
            get {
                if (idx < 0) {
                    throw new InvalidOperationException();
                }

                return l.KeyAt(l.Count - 1 - idx);
            }
        }

        object IEnumerator.Current => Current;

        internal KeyEnumerator(SortedList<TKey, TValue> l) {
            this.l = l;
            idx = NOT_STARTED;
            ver = l.modificationCount;
        }

        public void Dispose() {
            idx = NOT_STARTED;
        }

        public bool MoveNext() {
            if (ver != l.modificationCount) {
                throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
            }

            if (idx == NOT_STARTED) {
                idx = l.Count;
            }

            return idx != FINISHED && --idx != FINISHED;
        }

        void IEnumerator.Reset() {
            if (ver != l.modificationCount) {
                throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
            }

            idx = NOT_STARTED;
        }
    }

    struct ValueEnumerator : IEnumerator<TValue>, IDisposable {
        const int NOT_STARTED = -2;

        // this MUST be -1, because we depend on it in move next.
        // we just decr the size, so, 0 - 1 == FINISHED
        const int FINISHED = -1;

        readonly SortedList<TKey, TValue> l;
        int idx;
        readonly int ver;

        public TValue Current {
            get {
                if (idx < 0) {
                    throw new InvalidOperationException();
                }

                return l.ValueAt(l.Count - 1 - idx);
            }
        }

        object IEnumerator.Current => Current;

        internal ValueEnumerator(SortedList<TKey, TValue> l) {
            this.l = l;
            idx = NOT_STARTED;
            ver = l.modificationCount;
        }

        public void Dispose() {
            idx = NOT_STARTED;
        }

        public bool MoveNext() {
            if (ver != l.modificationCount) {
                throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
            }

            if (idx == NOT_STARTED) {
                idx = l.Count;
            }

            return idx != FINISHED && --idx != FINISHED;
        }

        void IEnumerator.Reset() {
            if (ver != l.modificationCount) {
                throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
            }

            idx = NOT_STARTED;
        }
    }

    class ListKeys : IList<TKey>, IReadOnlyList<TKey>, ICollection, IEnumerable {
        readonly SortedList<TKey, TValue> host;

        //
        // ICollection
        //

        public virtual int Count => host.Count;

        public virtual bool IsSynchronized => ((ICollection)host).IsSynchronized;

        public virtual bool IsReadOnly => true;

        public virtual object SyncRoot => ((ICollection)host).SyncRoot;

        public virtual TKey this[int index] {
            get => host.KeyAt(index);
            set => throw new NotSupportedException("attempt to modify a key");
        }

        public ListKeys(SortedList<TKey, TValue> host) {
            if (host == null) {
                throw new ArgumentNullException();
            }

            this.host = host;
        }

        // ICollection<TKey>

        public virtual void Add(TKey item) {
            throw new NotSupportedException();
        }

        public virtual bool Remove(TKey key) => throw new NotSupportedException();

        public virtual void Clear() {
            throw new NotSupportedException();
        }

        public virtual void CopyTo(TKey[] array, int arrayIndex) {
            if (host.Count == 0) {
                return;
            }

            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (arrayIndex >= array.Length) {
                throw new ArgumentOutOfRangeException("arrayIndex is greater than or equal to array.Length");
            }

            if (Count > array.Length - arrayIndex) {
                throw new ArgumentOutOfRangeException("Not enough space in array from arrayIndex to end of array");
            }

            var j = arrayIndex;
            for (var i = 0; i < Count; ++i) {
                array[j++] = host.KeyAt(i);
            }
        }

        public virtual bool Contains(TKey item) => host.IndexOfKey(item) > -1;

        //
        // IList<TKey>
        //
        public virtual int IndexOf(TKey item) => host.IndexOfKey(item);

        public virtual void Insert(int index, TKey item) {
            throw new NotSupportedException();
        }

        public virtual void RemoveAt(int index) {
            throw new NotSupportedException();
        }

        //
        // IEnumerable<TKey>
        //

        public virtual IEnumerator<TKey> GetEnumerator() =>
            /* We couldn't use yield as it does not support Reset () */
            new KeyEnumerator(host);

        public virtual void CopyTo(Array array, int arrayIndex) {
            host.CopyToArray(array, arrayIndex, EnumeratorMode.KEY_MODE);
        }

        //
        // IEnumerable
        //

        IEnumerator IEnumerable.GetEnumerator() {
            for (var i = 0; i < host.Count; ++i) {
                yield return host.KeyAt(i);
            }
        }
    }

    class ListValues : IList<TValue>, IReadOnlyList<TValue>, ICollection, IEnumerable {
        readonly SortedList<TKey, TValue> host;

        //
        // ICollection
        //

        public virtual int Count => host.Count;

        public virtual bool IsSynchronized => ((ICollection)host).IsSynchronized;

        public virtual bool IsReadOnly => true;

        public virtual object SyncRoot => ((ICollection)host).SyncRoot;

        public virtual TValue this[int index] {
            get => host.ValueAt(index);
            set => throw new NotSupportedException("attempt to modify a key");
        }

        public ListValues(SortedList<TKey, TValue> host) {
            if (host == null) {
                throw new ArgumentNullException();
            }

            this.host = host;
        }

        // ICollection<TValue>

        public virtual void Add(TValue item) {
            throw new NotSupportedException();
        }

        public virtual bool Remove(TValue value) => throw new NotSupportedException();

        public virtual void Clear() {
            throw new NotSupportedException();
        }

        public virtual void CopyTo(TValue[] array, int arrayIndex) {
            if (host.Count == 0) {
                return;
            }

            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (arrayIndex >= array.Length) {
                throw new ArgumentOutOfRangeException("arrayIndex is greater than or equal to array.Length");
            }

            if (Count > array.Length - arrayIndex) {
                throw new ArgumentOutOfRangeException("Not enough space in array from arrayIndex to end of array");
            }

            var j = arrayIndex;
            for (var i = 0; i < Count; ++i) {
                array[j++] = host.ValueAt(i);
            }
        }

        public virtual bool Contains(TValue item) => host.IndexOfValue(item) > -1;

        //
        // IList<TValue>
        //
        public virtual int IndexOf(TValue item) => host.IndexOfValue(item);

        public virtual void Insert(int index, TValue item) {
            throw new NotSupportedException();
        }

        public virtual void RemoveAt(int index) {
            throw new NotSupportedException();
        }

        //
        // IEnumerable<TValue>
        //

        public virtual IEnumerator<TValue> GetEnumerator() =>
            /* We couldn't use yield as it does not support Reset () */
            new ValueEnumerator(host);

        public virtual void CopyTo(Array array, int arrayIndex) {
            host.CopyToArray(array, arrayIndex, EnumeratorMode.VALUE_MODE);
        }

        //
        // IEnumerable
        //

        IEnumerator IEnumerable.GetEnumerator() {
            for (var i = 0; i < host.Count; ++i) {
                yield return host.ValueAt(i);
            }
        }
    }
} // SortedList
