namespace Vixen.Core.Yaml;

/// <summary>
///     Implements an indexer through an IEnumerator&lt;T&gt;.
/// </summary>
public class FakeList<T> {
    readonly IEnumerator<T> collection;
    int currentIndex = -1;

    /// <summary>
    ///     Gets the element at the specified index.
    /// </summary>
    /// <remarks>
    ///     If index is greater or equal than the last used index, this operation is O(index - lastIndex),
    ///     else this operation is O(index).
    /// </remarks>
    public T this[int index] {
        get {
            if (index < currentIndex) {
                collection.Reset();
                currentIndex = -1;
            }

            while (currentIndex < index) {
                if (!collection.MoveNext()) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                ++currentIndex;
            }

            return collection.Current;
        }
    }

    /// <summary>
    ///     Initializes a new instance of FakeList&lt;T&gt;.
    /// </summary>
    /// <param name="collection">The enumerator to use to implement the indexer.</param>
    public FakeList(IEnumerator<T> collection) {
        this.collection = collection;
    }

    /// <summary>
    ///     Initializes a new instance of FakeList&lt;T&gt;.
    /// </summary>
    /// <param name="collection">The collection to use to implement the indexer.</param>
    public FakeList(IEnumerable<T> collection) : this(collection.GetEnumerator()) { }
}
