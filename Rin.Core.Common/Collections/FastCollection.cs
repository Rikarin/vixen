using System.Collections;
using System.Runtime.InteropServices;

namespace Rin.Core.Collections;

/// <summary>
///     Faster and lighter implementation of <see cref="System.Collections.ObjectModel.Collection{T}" /> with value types
///     enumerators to avoid allocation in foreach loops, and various helper functions.
/// </summary>
/// <typeparam name="T">Type of elements of this collection </typeparam>
public class FastCollection<T> : IList<T>, IReadOnlyList<T> {
    const int DefaultCapacity = 4;

    T[] items;

    public int Capacity {
        get => items.Length;
        set {
            if (value != items.Length) {
                if (value > 0) {
                    var destinationArray = new T[value];
                    if (Count > 0) {
                        Array.Copy(items, 0, destinationArray, 0, Count);
                    }

                    items = destinationArray;
                } else {
                    items = Array.Empty<T>();
                }
            }
        }
    }

    public int Count { get; private set; }

    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    ///     Gets or sets the element <typeparamref name="T" /> at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>
    ///     The element <typeparamref name="T" />.
    /// </value>
    /// <returns>The element at the specified index</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">If index is out of range</exception>
    public T this[int index] {
        get {
            if (index < 0 || index >= Count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return items[index];
        }
        set {
            if (index < 0 || index >= Count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetItem(index, value);
        }
    }

    public FastCollection() {
        items = Array.Empty<T>();
    }

    public FastCollection(IEnumerable<T> collection) {
        if (collection is ICollection<T> is2) {
            var count = is2.Count;
            items = new T[count];
            is2.CopyTo(items, 0);
            Count = count;
        } else {
            Count = 0;
            items = new T[DefaultCapacity];
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext()) {
                Add(enumerator.Current);
            }
        }
    }

    public FastCollection(int capacity) {
        items = new T[capacity];
    }

    public void Add(T item) {
        InsertItem(Count, item);
    }

    public void Clear() {
        ClearItems();
    }

    public bool Contains(T item) {
        if (item == null) {
            for (var j = 0; j < Count; j++) {
                if (items[j] == null) {
                    return true;
                }
            }

            return false;
        }

        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++) {
            if (comparer.Equals(items[i], item)) {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex) {
        Array.Copy(items, 0, array, arrayIndex, Count);
    }

    public int IndexOf(T item) => Array.IndexOf(items, item, 0, Count);

    public void Insert(int index, T item) {
        if (index < 0 || index > Count) {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        InsertItem(index, item);
    }

    public bool Remove(T item) {
        var index = IndexOf(item);
        if (index >= 0) {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index) {
        if (index < 0 || index >= Count) {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        RemoveItem(index);
    }

    /// <summary>
    ///     Adds the elements of the specified source to the end of <see cref="FastCollection{T}" />.
    /// </summary>
    /// <param name="itemsArgs">The items to add to this collection.</param>
    public void AddRange<TE>(TE itemsArgs) where TE : IEnumerable<T> {
        foreach (var item in itemsArgs) {
            Add(item);
        }
    }

    /// <summary>
    ///     Inline Enumerator used directly by foreach.
    /// </summary>
    /// <returns>An enumerator of this collection</returns>
    public Enumerator GetEnumerator() => new(this);

    public void Sort() {
        Sort(0, Count, null);
    }

    public void Sort(IComparer<T> comparer) {
        Sort(0, Count, comparer);
    }

    public void Sort(int index, int count, IComparer<T>? comparer) {
        Array.Sort(items, index, count, comparer);
    }

    public void EnsureCapacity(int min) {
        if (items.Length < min) {
            var num = items.Length == 0 ? DefaultCapacity : items.Length * 2;
            if (num < min) {
                num = min;
            }

            Capacity = num;
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    protected virtual void ClearItems() {
        if (Count > 0) {
            Array.Clear(items, 0, Count);
        }

        Count = 0;
    }

    protected virtual void InsertItem(int index, T item) {
        if (Count == items.Length) {
            EnsureCapacity(Count + 1);
        }

        if (index < Count) {
            Array.Copy(items, index, items, index + 1, Count - index);
        }

        items[index] = item;
        Count++;
    }

    protected virtual void RemoveItem(int index) {
        Count--;
        if (index < Count) {
            Array.Copy(items, index + 1, items, index, Count - index);
        }

        items[Count] = default;
    }

    protected virtual void SetItem(int index, T item) {
        items[index] = item;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator {
        readonly FastCollection<T> list;
        int index;

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        internal Enumerator(FastCollection<T> list) {
            this.list = list;
            index = 0;
            Current = default;
        }

        public void Dispose() { }

        public bool MoveNext() {
            var list = this.list;
            if (index < list.Count) {
                Current = list.items[index];
                index++;
                return true;
            }

            return MoveNextRare();
        }

        bool MoveNextRare() {
            index = list.Count + 1;
            Current = default;
            return false;
        }

        void IEnumerator.Reset() {
            index = 0;
            Current = default;
        }
    }
}
