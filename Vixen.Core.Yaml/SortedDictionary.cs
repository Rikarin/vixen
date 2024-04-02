using System.Collections;
using System.Diagnostics;

namespace Vixen.Core.Yaml;

[DebuggerDisplay("Count = {Count}")]
class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary {
    KeyCollection? keys;
    ValueCollection? values;

    readonly TreeSet<KeyValuePair<TKey, TValue>> _set;

    public int Count => _set.Count;

    public IComparer<TKey> Comparer => ((KeyValuePairComparer)_set.Comparer).keyComparer;
    public KeyCollection Keys => keys ??= new(this);
    public ValueCollection Values => values ??= new(this);

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    ICollection IDictionary.Keys => Keys;

    ICollection IDictionary.Values => Values;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection)_set).SyncRoot;

    public TValue this[TKey key] {
        get {
            if (key == null) {
                throw new ArgumentNullException();
            }

            var node =
                _set.FindNode(new(key, default));
            if (node == null) {
                throw new KeyNotFoundException();
            }

            return node.Item.Value;
        }
        set {
            if (key == null) {
                throw new ArgumentNullException();
            }

            var node =
                _set.FindNode(new(key, default));
            if (node == null) {
                _set.Add(new(key, value));
            } else {
                node.Item = new(node.Item.Key, value);
                _set.UpdateVersion();
            }
        }
    }

    object IDictionary.this[object key] {
        get {
            if (IsCompatibleKey(key)) {
                if (TryGetValue((TKey)key, out var value)) {
                    return value;
                }
            }

            return null;
        }
        set {
            VerifyKey(key);
            VerifyValueType(value);
            this[(TKey)key] = (TValue)value;
        }
    }

    public SortedDictionary() : this((IComparer<TKey>)null) { }

    public SortedDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

    public SortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer) {
        if (dictionary == null) {
            throw new ArgumentNullException();
        }

        _set = new(new KeyValuePairComparer(comparer));

        foreach (var pair in dictionary) {
            _set.Add(pair);
        }
    }

    public SortedDictionary(IComparer<TKey> comparer) {
        _set = new(new KeyValuePairComparer(comparer));
    }

    public void Add(TKey key, TValue value) {
        if (key == null) {
            throw new ArgumentNullException();
        }

        _set.Add(new(key, value));
    }

    public void Clear() {
        _set.Clear();
    }

    public bool ContainsKey(TKey key) {
        if (key == null) {
            throw new ArgumentNullException();
        }

        return _set.Contains(new(key, default));
    }

    public bool ContainsValue(TValue value) {
        var found = false;
        if (value == null) {
            _set.InOrderTreeWalk(
                delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) {
                    if (node.Item.Value == null) {
                        found = true;
                        return false; // stop the walk
                    }

                    return true;
                }
            );
        } else {
            var valueComparer = EqualityComparer<TValue>.Default;
            _set.InOrderTreeWalk(
                delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) {
                    if (valueComparer.Equals(node.Item.Value, value)) {
                        found = true;
                        return false; // stop the walk
                    }

                    return true;
                }
            );
        }

        return found;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index) {
        _set.CopyTo(array, index);
    }

    public Enumerator GetEnumerator() => new(this, Enumerator.KeyValuePair);

    public bool Remove(TKey key) {
        if (key == null) {
            throw new ArgumentNullException();
        }

        return _set.Remove(new(key, default));
    }

    public bool TryGetValue(TKey key, out TValue value) {
        if (key == null) {
            throw new ArgumentNullException();
        }

        var node =
            _set.FindNode(new(key, default));
        if (node == null) {
            value = default;
            return false;
        }

        value = node.Item.Value;
        return true;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) {
        _set.Add(keyValuePair);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair) {
        var node = _set.FindNode(keyValuePair);
        if (node == null) {
            return false;
        }

        if (keyValuePair.Value == null) {
            return node.Item.Value == null;
        }

        return EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) {
        var node = _set.FindNode(keyValuePair);
        if (node == null) {
            return false;
        }

        if (EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value)) {
            _set.Remove(keyValuePair);
            return true;
        }

        return false;
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
        new Enumerator(this, Enumerator.KeyValuePair);

    void ICollection.CopyTo(Array array, int index) {
        ((ICollection)_set).CopyTo(array, index);
    }

    void IDictionary.Add(object key, object value) {
        VerifyKey(key);
        VerifyValueType(value);
        Add((TKey)key, (TValue)value);
    }

    bool IDictionary.Contains(object key) {
        if (IsCompatibleKey(key)) {
            return ContainsKey((TKey)key);
        }

        return false;
    }

    static void VerifyKey(object key) {
        if (key == null) {
            throw new ArgumentNullException();
        }

        if (key is not TKey) {
            throw new ArgumentException();
        }
    }

    static bool IsCompatibleKey(object key) {
        if (key == null) {
            throw new ArgumentNullException();
        }

        return key is TKey;
    }

    static void VerifyValueType(object? value) {
        if (value is TValue || (value == null && !typeof(TValue).IsValueType)) {
            return;
        }

        throw new ArgumentNullException();
    }

    IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

    void IDictionary.Remove(object key) {
        if (IsCompatibleKey(key)) {
            Remove((TKey)key);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator {
        internal const int KeyValuePair = 1;
        internal const int DictEntry = 2;
        TreeSet<KeyValuePair<TKey, TValue>>.Enumerator treeEnum;
        readonly int getEnumeratorRetType; // What should Enumerator.Current return?

        public KeyValuePair<TKey, TValue> Current => treeEnum.Current;

        internal bool NotStartedOrEnded => treeEnum.NotStartedOrEnded;

        object IEnumerator.Current {
            get {
                if (NotStartedOrEnded) {
                    throw new InvalidOperationException();
                }

                if (getEnumeratorRetType == DictEntry) {
                    return new DictionaryEntry(Current.Key, Current.Value);
                }

                return new KeyValuePair<TKey, TValue>(Current.Key, Current.Value);
            }
        }

        object IDictionaryEnumerator.Key {
            get {
                if (NotStartedOrEnded) {
                    throw new InvalidOperationException();
                }

                return Current.Key;
            }
        }

        object IDictionaryEnumerator.Value {
            get {
                if (NotStartedOrEnded) {
                    throw new InvalidOperationException();
                }

                return Current.Value;
            }
        }

        DictionaryEntry IDictionaryEnumerator.Entry {
            get {
                if (NotStartedOrEnded) {
                    throw new InvalidOperationException();
                }

                return new(Current.Key, Current.Value);
            }
        }

        internal Enumerator(SortedDictionary<TKey, TValue> dictionary, int getEnumeratorRetType) {
            treeEnum = dictionary._set.GetEnumerator();
            this.getEnumeratorRetType = getEnumeratorRetType;
        }

        public bool MoveNext() => treeEnum.MoveNext();

        public void Dispose() {
            treeEnum.Dispose();
        }


        void IEnumerator.Reset() {
            treeEnum.Reset();
        }

        internal void Reset() {
            treeEnum.Reset();
        }
    }

    public sealed class KeyCollection : ICollection<TKey>, ICollection {
        readonly SortedDictionary<TKey, TValue> dictionary;

        public int Count => dictionary.Count;

        bool ICollection<TKey>.IsReadOnly => true;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

        public KeyCollection(SortedDictionary<TKey, TValue>? dictionary) {
            this.dictionary = dictionary ?? throw new ArgumentNullException();
        }

        public Enumerator GetEnumerator() => new(dictionary);

        public void CopyTo(TKey[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException();
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < Count) {
                throw new ArgumentException();
            }

            dictionary._set.InOrderTreeWalk(
                delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) {
                    array[index++] = node.Item.Key;
                    return true;
                }
            );
        }

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => new Enumerator(dictionary);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(dictionary);

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException();
            }

            if (array.Rank != 1) {
                throw new ArgumentException();
            }

            if (array.GetLowerBound(0) != 0) {
                throw new ArgumentException();
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < dictionary.Count) {
                throw new ArgumentException();
            }

            if (array is TKey[] keys) {
                CopyTo(keys, index);
            } else {
                var objects = (object[])array;
                if (objects == null) {
                    throw new ArgumentException();
                }

                try {
                    dictionary._set.InOrderTreeWalk(
                        delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) {
                            objects[index++] = node.Item.Key;
                            return true;
                        }
                    );
                } catch (ArrayTypeMismatchException) {
                    throw new ArgumentException();
                }
            }
        }

        void ICollection<TKey>.Add(TKey item) {
            throw new NotSupportedException();
        }

        void ICollection<TKey>.Clear() {
            throw new NotSupportedException();
        }

        bool ICollection<TKey>.Contains(TKey item) => dictionary.ContainsKey(item);

        bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();

        public struct Enumerator : IEnumerator<TKey>, IEnumerator {
            SortedDictionary<TKey, TValue>.Enumerator dictEnum;

            public TKey Current => dictEnum.Current.Key;

            object IEnumerator.Current {
                get {
                    if (dictEnum.NotStartedOrEnded) {
                        throw new InvalidOperationException();
                    }

                    return Current;
                }
            }

            internal Enumerator(SortedDictionary<TKey, TValue> dictionary) {
                dictEnum = dictionary.GetEnumerator();
            }

            public void Dispose() {
                dictEnum.Dispose();
            }

            public bool MoveNext() => dictEnum.MoveNext();

            void IEnumerator.Reset() {
                dictEnum.Reset();
            }
        }
    }

    public sealed class ValueCollection : ICollection<TValue>, ICollection {
        readonly SortedDictionary<TKey, TValue> dictionary;

        public int Count => dictionary.Count;

        bool ICollection<TValue>.IsReadOnly => true;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

        public ValueCollection(SortedDictionary<TKey, TValue>? dictionary) {
            this.dictionary = dictionary ?? throw new ArgumentNullException();
        }

        public Enumerator GetEnumerator() => new(dictionary);

        public void CopyTo(TValue[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException();
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < Count) {
                throw new NotSupportedException();
            }

            dictionary._set.InOrderTreeWalk(
                delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) {
                    array[index++] = node.Item.Value;
                    return true;
                }
            );
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new Enumerator(dictionary);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(dictionary);

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentException();
            }

            if (array.Rank != 1) {
                throw new ArgumentException();
            }

            if (array.GetLowerBound(0) != 0) {
                throw new ArgumentException();
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < dictionary.Count) {
                throw new ArgumentException();
            }

            if (array is TValue[] values) {
                CopyTo(values, index);
            } else {
                var objects = (object[])array;
                if (objects == null) {
                    throw new ArgumentException();
                }

                try {
                    dictionary._set.InOrderTreeWalk(
                        delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) {
                            objects[index++] = node.Item.Value;
                            return true;
                        }
                    );
                } catch (ArrayTypeMismatchException) {
                    throw new ArgumentException();
                }
            }
        }

        void ICollection<TValue>.Add(TValue item) {
            throw new NotSupportedException();
        }

        void ICollection<TValue>.Clear() {
            throw new NotSupportedException();
        }

        bool ICollection<TValue>.Contains(TValue item) => dictionary.ContainsValue(item);

        bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();

        public struct Enumerator : IEnumerator<TValue>, IEnumerator {
            SortedDictionary<TKey, TValue>.Enumerator dictEnum;

            public TValue Current => dictEnum.Current.Value;

            object IEnumerator.Current {
                get {
                    if (dictEnum.NotStartedOrEnded) {
                        throw new InvalidOperationException();
                    }

                    return Current;
                }
            }

            internal Enumerator(SortedDictionary<TKey, TValue> dictionary) {
                dictEnum = dictionary.GetEnumerator();
            }

            public void Dispose() {
                dictEnum.Dispose();
            }

            public bool MoveNext() => dictEnum.MoveNext();

            void IEnumerator.Reset() {
                dictEnum.Reset();
            }
        }
    }

    internal class KeyValuePairComparer : Comparer<KeyValuePair<TKey, TValue>> {
        internal IComparer<TKey> keyComparer;

        public KeyValuePairComparer(IComparer<TKey> keyComparer) {
            if (keyComparer == null) {
                this.keyComparer = Comparer<TKey>.Default;
            } else {
                this.keyComparer = keyComparer;
            }
        }

        public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
            keyComparer.Compare(x.Key, y.Key);
    }
}

//
// A binary search tree is a red-black tree if it satisfies the following red-black properties:
// 1. Every node is either red or black
// 2. Every leaf (nil node) is black
// 3. If a node is red, the both its children are black
// 4. Every simple path from a node to a descendant leaf contains the same number of black nodes
// 
// The basic idea of red-black tree is to represent 2-3-4 trees as standard BSTs but to add one extra bit of information  
// per node to encode 3-nodes and 4-nodes. 
// 4-nodes will be represented as:		  B
//															  R			R
// 3 -node will be represented as:		   B			 or		 B	 
//															  R		  B			   B	   R
// 
// For a detailed description of the algorithm, take a look at "Algorithm" by Rebert Sedgewick.
//

delegate bool TreeWalkAction<T>(TreeSet<T>.Node node);

enum TreeRotation {
    LeftRotation = 1,
    RightRotation = 2,
    RightLeftRotation = 3,
    LeftRightRotation = 4
}

class TreeSet<T> : ICollection<T>, ICollection {
    const string ComparerName = "Comparer";
    const string CountName = "Count";
    const string ItemsName = "Items";
    const string VersionName = "Version";
    Node root;
    int version;
    object _syncRoot;

    public int Count { get; private set; }

    public IComparer<T> Comparer { get; }

    bool ICollection<T>.IsReadOnly => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot {
        get {
            if (_syncRoot == null) {
                Interlocked.CompareExchange(ref _syncRoot, new(), null);
            }

            return _syncRoot;
        }
    }

    public TreeSet(IComparer<T> comparer) {
        Comparer = comparer ?? Comparer<T>.Default;
    }

    public void Add(T item) {
        if (root == null) {
            // empty tree
            root = new(item, false);
            Count = 1;
            return;
        }

        //
        // Search for a node at bottom to insert the new node. 
        // If we can guanratee the node we found is not a 4-node, it would be easy to do insertion.
        // We split 4-nodes along the search path.
        // 
        var current = root;
        Node parent = null;
        Node grandParent = null;
        Node greatGrandParent = null;

        var order = 0;
        while (current != null) {
            order = Comparer.Compare(item, current.Item);
            if (order == 0) {
                // We could have changed root node to red during the search process.
                // We need to set it to black before we return.
                root.IsRed = false;
                throw new ArgumentException();
            }

            // split a 4-node into two 2-nodes				
            if (Is4Node(current)) {
                Split4Node(current);
                // We could have introduced two consecutive red nodes after split. Fix that by rotation.
                if (IsRed(parent)) {
                    InsertionBalance(current, ref parent, grandParent, greatGrandParent);
                }
            }

            greatGrandParent = grandParent;
            grandParent = parent;
            parent = current;
            current = order < 0 ? current.Left : current.Right;
        }

        Debug.Assert(parent != null, "Parent node cannot be null here!");
        // ready to insert the new node
        var node = new Node(item);
        if (order > 0) {
            parent.Right = node;
        } else {
            parent.Left = node;
        }

        // the new node will be red, so we will need to adjust the colors if parent node is also red
        if (parent.IsRed) {
            InsertionBalance(node, ref parent, grandParent, greatGrandParent);
        }

        // Root node is always black
        root.IsRed = false;
        ++Count;
        ++version;
    }

    public void Clear() {
        root = null;
        Count = 0;
        ++version;
    }

    public bool Contains(T item) => FindNode(item) != null;

    public void CopyTo(T[] array, int index) {
        if (array == null) {
            throw new ArgumentNullException();
        }

        if (index < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (array.Length - index < Count) {
            throw new ArgumentException();
        }

        InOrderTreeWalk(
            delegate(Node node) {
                array[index++] = node.Item;
                return true;
            }
        );
    }

    public Enumerator GetEnumerator() => new(this);

    public bool Remove(T item) {
        if (root == null) {
            return false;
        }

        // Search for a node and then find its succesor. 
        // Then copy the item from the succesor to the matching node and delete the successor. 
        // If a node doesn't have a successor, we can replace it with its left child (if not empty.) 
        // or delete the matching node.
        // 
        // In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
        // Following code will make sure the node on the path is not a 2 Node. 
        // 
        var current = root;
        Node parent = null;
        Node grandParent = null;
        Node match = null;
        Node parentOfMatch = null;
        var foundMatch = false;
        while (current != null) {
            if (Is2Node(current)) {
                // fix up 2-Node
                if (parent == null) {
                    // current is root. Mark it as red
                    current.IsRed = true;
                } else {
                    var sibling = GetSibling(current, parent);
                    if (sibling.IsRed) {
                        // If parent is a 3-node, flip the orientation of the red link. 
                        // We can acheive this by a single rotation		
                        // This case is converted to one of other cased below.
                        Debug.Assert(!parent.IsRed, "parent must be a black node!");
                        if (parent.Right == sibling) {
                            RotateLeft(parent);
                        } else {
                            RotateRight(parent);
                        }

                        parent.IsRed = true;
                        sibling.IsRed = false; // parent's color
                        // sibling becomes child of grandParent or root after rotation. Update link from grandParent or root
                        ReplaceChildOfNodeOrRoot(grandParent, parent, sibling);
                        // sibling will become grandParent of current node 
                        grandParent = sibling;
                        if (parent == match) {
                            parentOfMatch = sibling;
                        }

                        // update sibling, this is necessary for following processing
                        sibling = parent.Left == current ? parent.Right : parent.Left;
                    }

                    Debug.Assert(
                        sibling != null || sibling.IsRed == false,
                        "sibling must not be null and it must be black!"
                    );

                    if (Is2Node(sibling)) {
                        Merge2Nodes(parent, current, sibling);
                    } else {
                        // current is a 2-node and sibling is either a 3-node or a 4-node.
                        // We can change the color of current to red by some rotation.
                        var rotation = RotationNeeded(parent, current, sibling);
                        Node newGrandParent = null;
                        switch (rotation) {
                            case TreeRotation.RightRotation:
                                Debug.Assert(parent.Left == sibling, "sibling must be left child of parent!");
                                Debug.Assert(sibling.Left.IsRed, "Left child of sibling must be red!");
                                sibling.Left.IsRed = false;
                                newGrandParent = RotateRight(parent);
                                break;
                            case TreeRotation.LeftRotation:
                                Debug.Assert(parent.Right == sibling, "sibling must be left child of parent!");
                                Debug.Assert(sibling.Right.IsRed, "Right child of sibling must be red!");
                                sibling.Right.IsRed = false;
                                newGrandParent = RotateLeft(parent);
                                break;

                            case TreeRotation.RightLeftRotation:
                                Debug.Assert(parent.Right == sibling, "sibling must be left child of parent!");
                                Debug.Assert(sibling.Left.IsRed, "Left child of sibling must be red!");
                                newGrandParent = RotateRightLeft(parent);
                                break;

                            case TreeRotation.LeftRightRotation:
                                Debug.Assert(parent.Left == sibling, "sibling must be left child of parent!");
                                Debug.Assert(sibling.Right.IsRed, "Right child of sibling must be red!");
                                newGrandParent = RotateLeftRight(parent);
                                break;
                        }

                        newGrandParent.IsRed = parent.IsRed;
                        parent.IsRed = false;
                        current.IsRed = true;
                        ReplaceChildOfNodeOrRoot(grandParent, parent, newGrandParent);
                        if (parent == match) {
                            parentOfMatch = newGrandParent;
                        }

                        grandParent = newGrandParent;
                    }
                }
            }

            // we don't need to compare any more once we found the match
            var order = foundMatch ? -1 : Comparer.Compare(item, current.Item);
            if (order == 0) {
                // save the matching node
                foundMatch = true;
                match = current;
                parentOfMatch = parent;
            }

            grandParent = parent;
            parent = current;

            if (order < 0) {
                current = current.Left;
            } else {
                current = current.Right; // continue the search in  right sub tree after we find a match
            }
        }

        // move successor to the matching node position and replace links
        if (match != null) {
            ReplaceNode(match, parentOfMatch, parent, grandParent);
            --Count;
        }

        if (root != null) {
            root.IsRed = false;
        }

        ++version;
        return foundMatch;
    }

    void ICollection.CopyTo(Array array, int index) {
        if (array == null) {
            throw new ArgumentNullException();
        }

        if (array.Rank != 1) {
            throw new ArgumentException();
        }

        if (array.GetLowerBound(0) != 0) {
            throw new ArgumentException();
        }

        if (index < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (array.Length - index < Count) {
            throw new ArgumentException();
        }

        if (array is T[] tArray) {
            CopyTo(tArray, index);
        } else {
            if (array is not object[] objects) {
                throw new ArgumentException();
            }

            try {
                InOrderTreeWalk(
                    delegate(Node node) {
                        objects[index++] = node.Item;
                        return true;
                    }
                );
            } catch (ArrayTypeMismatchException) {
                throw new ArgumentException();
            }
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);


    static Node GetSibling(Node node, Node parent) {
        if (parent.Left == node) {
            return parent.Right;
        }

        return parent.Left;
    }

    // After calling InsertionBalance, we need to make sure current and parent up-to-date.
    // It doesn't matter if we keep grandParent and greatGrantParent up-to-date 
    // because we won't need to split again in the next node.
    // By the time we need to split again, everything will be correctly set.
    //  
    void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent) {
        Debug.Assert(grandParent != null, "Grand parent cannot be null here!");
        var parentIsOnRight = grandParent.Right == parent;
        var currentIsOnRight = parent.Right == current;

        Node newChildOfGreatGrandParent;
        if (parentIsOnRight == currentIsOnRight) {
            // same orientation, single rotation
            newChildOfGreatGrandParent = currentIsOnRight ? RotateLeft(grandParent) : RotateRight(grandParent);
        } else {
            // different orientation, double rotation
            newChildOfGreatGrandParent = currentIsOnRight
                ? RotateLeftRight(grandParent)
                : RotateRightLeft(grandParent);
            // current node now becomes the child of great grandparent 
            parent = greatGrandParent;
        }

        // grand parent will become a child of either parent of current.
        grandParent.IsRed = true;
        newChildOfGreatGrandParent.IsRed = false;

        ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
    }

    static bool Is2Node(Node node) {
        Debug.Assert(node != null, "node cannot be null!");
        return IsBlack(node) && IsNullOrBlack(node.Left) && IsNullOrBlack(node.Right);
    }

    static bool Is4Node(Node node) => IsRed(node.Left) && IsRed(node.Right);

    static bool IsBlack(Node node) => node is { IsRed: false };

    static bool IsNullOrBlack(Node node) => node is not { IsRed: true };

    static bool IsRed(Node node) => node is { IsRed: true };

    static void Merge2Nodes(Node parent, Node child1, Node child2) {
        Debug.Assert(IsRed(parent), "parent must be be red");
        // combing two 2-nodes into a 4-node
        parent.IsRed = false;
        child1.IsRed = true;
        child2.IsRed = true;
    }

    // Replace the child of a parent node. 
    // If the parent node is null, replace the root.
    void ReplaceChildOfNodeOrRoot(Node parent, Node child, Node newChild) {
        if (parent != null) {
            if (parent.Left == child) {
                parent.Left = newChild;
            } else {
                parent.Right = newChild;
            }
        } else {
            root = newChild;
        }
    }

    // Replace the matching node with its successor.
    void ReplaceNode(Node match, Node parentOfMatch, Node successor, Node parentOfSuccessor) {
        if (successor == match) {
            // this node has no successor, should only happen if right child of matching node is null.
            Debug.Assert(match.Right == null, "Right child must be null!");
            successor = match.Left;
        } else {
            Debug.Assert(parentOfSuccessor != null, "parent of successor cannot be null!");
            Debug.Assert(successor.Left == null, "Left child of successor must be null!");
            Debug.Assert(
                (successor.Right == null && successor.IsRed) || (successor.Right.IsRed && !successor.IsRed),
                "Succesor must be in valid state"
            );
            if (successor.Right != null) {
                successor.Right.IsRed = false;
            }

            if (parentOfSuccessor != match) {
                // detach successor from its parent and set its right child
                parentOfSuccessor.Left = successor.Right;
                successor.Right = match.Right;
            }

            successor.Left = match.Left;
        }

        if (successor != null) {
            successor.IsRed = match.IsRed;
        }

        ReplaceChildOfNodeOrRoot(parentOfMatch, match, successor);
    }

    static Node RotateLeft(Node node) {
        var x = node.Right;
        node.Right = x.Left;
        x.Left = node;
        return x;
    }

    static Node RotateLeftRight(Node node) {
        var child = node.Left;
        var grandChild = child.Right;

        node.Left = grandChild.Right;
        grandChild.Right = node;
        child.Right = grandChild.Left;
        grandChild.Left = child;
        return grandChild;
    }

    static Node RotateRight(Node node) {
        var x = node.Left;
        node.Left = x.Right;
        x.Right = node;
        return x;
    }

    static Node RotateRightLeft(Node node) {
        var child = node.Right;
        var grandChild = child.Left;

        node.Right = grandChild.Left;
        grandChild.Left = node;
        child.Left = grandChild.Right;
        grandChild.Right = child;
        return grandChild;
    }

    static TreeRotation RotationNeeded(Node parent, Node current, Node sibling) {
        Debug.Assert(IsRed(sibling.Left) || IsRed(sibling.Right), "sibling must have at least one red child");
        if (IsRed(sibling.Left)) {
            if (parent.Left == current) {
                return TreeRotation.RightLeftRotation;
            }

            return TreeRotation.RightRotation;
        }

        if (parent.Left == current) {
            return TreeRotation.LeftRotation;
        }

        return TreeRotation.LeftRightRotation;
    }

    static void Split4Node(Node node) {
        node.IsRed = true;
        node.Left.IsRed = false;
        node.Right.IsRed = false;
    }

    //
    // Do a in order walk on tree and calls the delegate for each node.
    // If the action delegate returns false, stop the walk.
    // 
    // Return true if the entire tree has been walked. 
    // Otherwise returns false.
    //
    internal bool InOrderTreeWalk(TreeWalkAction<T> action) {
        if (root == null) {
            return true;
        }

        // The maximum height of a red-black tree is 2*lg(n+1).
        // See page 264 of "Introduction to algorithms" by Thomas H. Cormen
        var stack = new Stack<Node>(2 * (int)Math.Log(Count + 1));
        var current = root;
        while (current != null) {
            stack.Push(current);
            current = current.Left;
        }

        while (stack.Count != 0) {
            current = stack.Pop();
            if (!action(current)) {
                return false;
            }

            var node = current.Right;
            while (node != null) {
                stack.Push(node);
                node = node.Left;
            }
        }

        return true;
    }

    internal Node FindNode(T item) {
        var current = root;
        while (current != null) {
            var order = Comparer.Compare(item, current.Item);
            if (order == 0) {
                return current;
            }

            current = order < 0 ? current.Left : current.Right;
        }

        return null;
    }

    internal void UpdateVersion() {
        ++version;
    }

    internal class Node {
        public T Item { get; set; }

        public Node Left { get; set; }

        public Node Right { get; set; }

        public bool IsRed { get; set; }

        public Node(T item) {
            // The default color will be red, we never need to create a black node directly.				
            Item = item;
            IsRed = true;
        }

        public Node(T item, bool isRed) {
            // The default color will be red, we never need to create a black node directly.				
            Item = item;
            IsRed = isRed;
        }
    }

    public struct Enumerator : IEnumerator<T>, IEnumerator {
        const string TreeName = "Tree";
        const string NodeValueName = "Item";
        const string EnumStartName = "EnumStarted";
        const string VersionName = "Version";
        readonly TreeSet<T> tree;
        readonly int version;
        readonly Stack<Node> stack;
        Node? current;
        static Node dummyNode = new(default);

        public T Current => current != null ? current.Item : default;

        internal bool NotStartedOrEnded => current == null;

        object IEnumerator.Current {
            get {
                if (current == null) {
                    throw new InvalidOperationException();
                }

                return current.Item;
            }
        }

        internal Enumerator(TreeSet<T> set) {
            tree = set;
            version = tree.version;

            // 2lg(n + 1) is the maximum height
            stack = new(2 * (int)Math.Log(set.Count + 1));
            current = null;
            Intialize();
        }

        public bool MoveNext() {
            if (version != tree.version) {
                throw new InvalidOperationException();
            }

            if (stack.Count == 0) {
                current = null;
                return false;
            }

            current = stack.Pop();
            var node = current.Right;
            while (node != null) {
                stack.Push(node);
                node = node.Left;
            }

            return true;
        }

        public void Dispose() { }

        void Intialize() {
            current = null;
            var node = tree.root;
            while (node != null) {
                stack.Push(node);
                node = node.Left;
            }
        }

        void IEnumerator.Reset() {
            Reset();
        }

        internal void Reset() {
            if (version != tree.version) {
                throw new InvalidOperationException();
            }

            stack.Clear();
            Intialize();
        }
    }
}
