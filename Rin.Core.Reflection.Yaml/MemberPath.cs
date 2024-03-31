using Rin.Core.Reflection.MemberDescriptors;
using Rin.Core.Reflection.TypeDescriptors;
using System.Text;

namespace Rin.Core.Reflection;

/// <summary>
///     Allows to get/set a property/field value on a deeply nested object instance (supporting
///     members, list access and dictionary access)
/// </summary>
// TODO: This data contract has been added because we are using a PropertyKey<MemberPath> somewhere, and the assembly processor expect the generic type of PropertyKey to be serializable. MemberPath is actually not serializable. We need to allow to use PropertyContainer/Key without serializable object.
[DataContract]
public sealed class MemberPath {
    /// <summary>
    ///     We use a thread local static to avoid allocating a list of reference objects every time we access a property
    /// </summary>
    [ThreadStatic]
    static List<object>? stackTLS;

    readonly List<MemberPathItem> items;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPath" /> class.
    /// </summary>
    public MemberPath() : this(16) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPath" /> class.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public MemberPath(int capacity) {
        items = new(capacity);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPath" /> class.
    /// </summary>
    /// <param name="items">The items.</param>
    MemberPath(List<MemberPathItem> items) {
        if (items == null) {
            throw new ArgumentNullException(nameof(items));
        }

        this.items = new(items.Capacity);
        foreach (var item in items) {
            this.items.Add(item.Clone(this.items.LastOrDefault()));
        }
    }

    /// <summary>
    ///     Checks whether the given <see cref="MemberPath" /> matches with this instance.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><c>true</c> if the given <see cref="MemberPath" /> matches with this instance; otherwise, <c>false</c>.</returns>
    public bool Match(MemberPath other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        if (items.Count != other.items.Count) {
            return false;
        }

        return !items.Where((t, i) => !t.Equals(other.items[i])).Any();
    }

    /// <summary>
    ///     Ensures the capacity of the paths definition when using
    ///     <see cref="Push(IMemberDescriptor)" /> methods.
    /// </summary>
    /// <param name="pathCount">The path count.</param>
    public void EnsureCapacity(int pathCount) {
        items.Capacity = pathCount;
    }

    /// <summary>
    ///     Clears the current path.
    /// </summary>
    public void Clear() {
        items.Clear();
    }

    /// <summary>
    ///     Gets the custom attribute of the last property/field from this member path.
    /// </summary>
    /// <typeparam name="T">Type of the attribute</typeparam>
    /// <returns>A custom attribute or null if not found</returns>
    public T GetCustomAttribute<T>() where T : Attribute {
        if (items == null || items.Count == 0) {
            return null;
        }

        for (var i = items.Count - 1; i >= 0; i--) {
            var descriptor = items[i].MemberDescriptor;
            if (descriptor == null) {
                continue;
            }

            var attributes = descriptor.GetCustomAttributes<T>(false);
            return attributes.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    ///     Appends the given <paramref name="path" /> to this instance.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>This instance.</returns>
    public MemberPath Append(MemberPath path) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        foreach (var item in path.items) {
            AddItem(item.Clone(null));
        }

        return this;
    }

    /// <summary>
    ///     Pushes a member access on the path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the member.</param>
    /// <exception cref="System.ArgumentNullException">descriptor</exception>
    public void Push(IMemberDescriptor descriptor) {
        if (descriptor == null) {
            throw new ArgumentNullException(nameof(descriptor));
        }

        AddItem(
            descriptor is FieldDescriptor
                ? new FieldPathItem((FieldDescriptor)descriptor)
                : new PropertyPathItem((PropertyDescriptor)descriptor)
        );
    }

    public void Push(ITypeDescriptor descriptor, object key) {
        if (descriptor is ArrayDescriptor arrayDescriptor) {
            Push(arrayDescriptor, (int)key);
        } else if (descriptor is CollectionDescriptor collectionDescriptor) {
            if (descriptor.Category == DescriptorCategory.Set) {
                Push(collectionDescriptor as SetDescriptor, key);
            } else {
                Push(collectionDescriptor, (int)key);
            }
        } else if (descriptor is DictionaryDescriptor dictionaryDescriptor) {
            Push(dictionaryDescriptor, key);
        }
    }

    /// <summary>
    ///     Pushes an array access on the path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the array.</param>
    /// <param name="index">The index in the array.</param>
    /// <exception cref="System.ArgumentNullException">descriptor</exception>
    public void Push(ArrayDescriptor descriptor, int index) {
        if (descriptor == null) {
            throw new ArgumentNullException(nameof(descriptor));
        }

        AddItem(new ArrayPathItem(descriptor, index));
    }

    /// <summary>
    ///     Pushes an collection access on the path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the collection.</param>
    /// <param name="index">The index in the collection.</param>
    /// <exception cref="System.ArgumentNullException">descriptor</exception>
    public void Push(CollectionDescriptor descriptor, int index) {
        if (descriptor == null) {
            throw new ArgumentNullException(nameof(descriptor));
        }

        AddItem(new CollectionPathItem(descriptor, index));
    }

    /// <summary>
    ///     Pushes an dictionary access on the path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the dictionary.</param>
    /// <param name="key">The key.</param>
    /// <exception cref="System.ArgumentNullException">descriptor</exception>
    public void Push(DictionaryDescriptor descriptor, object key) {
        if (descriptor == null) {
            throw new ArgumentNullException(nameof(descriptor));
        }

        AddItem(new DictionaryPathItem(descriptor, key));
    }

    /// <summary>
    ///     Pushes an collection access on the path.
    /// </summary>
    /// <param name="descriptor">The descriptor of the set.</param>
    /// <param name="index">The index in the set.</param>
    /// <exception cref="System.ArgumentNullException">descriptor</exception>
    public void Push(SetDescriptor descriptor, object index) {
        if (descriptor == null) {
            throw new ArgumentNullException(nameof(descriptor));
        }

        AddItem(new SetPathItem(descriptor, index));
    }

    /// <summary>
    ///     Pops the last item from the current path.
    /// </summary>
    public void Pop() {
        if (items.Count > 0) {
            items.RemoveAt(items.Count - 1);
        }
    }

    public bool Apply(object rootObject, MemberPathAction actionType, object value) {
        if (rootObject == null) {
            throw new ArgumentNullException(nameof(rootObject));
        }

        if (rootObject.GetType().IsValueType) {
            throw new ArgumentException("Value type for root objects are not supported", nameof(rootObject));
        }

        if (actionType != MemberPathAction.ValueSet && actionType != MemberPathAction.CollectionAdd && value != null) {
            throw new ArgumentException(
                "Value must be null for action != (MemberActionType.SetValue || MemberPathAction.CollectionAdd)"
            );
        }

        if (items == null || items.Count == 0) {
            throw new InvalidOperationException(
                "This instance doesn't contain any path. Use Push() methods to populate paths"
            );
        }

        var lastItem = items[^1];
        switch (actionType) {
            case MemberPathAction.CollectionAdd:
                if (lastItem is not CollectionPathItem) {
                    throw new ArgumentException(
                        $"Invalid path [{this}] for action [{actionType}]. Expecting last path to be a collection item"
                    );
                }

                break;
            case MemberPathAction.CollectionRemove:
                if (lastItem is not (CollectionPathItem or ArrayPathItem)) {
                    throw new ArgumentException(
                        $"Invalid path [{this}] for action [{actionType}]. Expecting last path to be a collection/array item"
                    );
                }

                break;

            case MemberPathAction.DictionaryRemove:
                if (lastItem is not DictionaryPathItem) {
                    throw new ArgumentException(
                        $"Invalid path [{this}] for action [{actionType}]. Expecting last path to be a dictionary item"
                    );
                }

                break;
        }

        var stack = stackTLS;
        try {
            var nextObject = rootObject;

            if (stack == null) {
                stack = [];
                stackTLS = stack;
            } else {
                stack.Clear();
            }

            stack.Add(nextObject);
            for (var i = 0; i < items.Count - 1; i++) {
                var item = items[i];
                nextObject = item.GetValue(nextObject);
                stack.Add(nextObject);
            }

            if (actionType == MemberPathAction.ValueClear) {
                if (lastItem is CollectionPathItem) {
                    actionType = lastItem.TypeDescriptor.Category == DescriptorCategory.Set
                        ? MemberPathAction.ValueSet
                        : MemberPathAction.CollectionRemove;
                } else if (lastItem is DictionaryPathItem) {
                    actionType = MemberPathAction.DictionaryRemove;
                } else {
                    actionType = MemberPathAction.ValueSet;
                }
            }

            switch (actionType) {
                case MemberPathAction.ValueSet:
                    lastItem.SetValue(stack, stack.Count - 1, nextObject, value);
                    break;

                case MemberPathAction.DictionaryRemove:
                    ((DictionaryPathItem)lastItem).Descriptor.Remove(nextObject, ((DictionaryPathItem)lastItem).Key);
                    break;

                case MemberPathAction.CollectionAdd:
                    ((CollectionPathItem)lastItem).Descriptor.Add(nextObject, value);
                    break;

                case MemberPathAction.CollectionRemove:
                    ((CollectionPathItem)lastItem).Descriptor.RemoveAt(
                        nextObject,
                        ((CollectionPathItem)lastItem).Index
                    );
                    break;
            }
        } catch (Exception) {
            // If an exception occurred, we cannot resolve this member path to a valid property/field
            return false;
        } finally {
            stack?.Clear();
        }

        return true;
    }

    public object GetIndex() => items.LastOrDefault()?.GetIndex();

    /// <summary>
    ///     Gets the type descriptor of the member or collection represented by this path, or <c>null</c> is this instance is
    ///     an empty path.
    /// </summary>
    /// <returns>
    ///     The type descriptor of the member or collection represented by this path, or <c>null</c> is this instance is
    ///     an empty path.
    /// </returns>
    public ITypeDescriptor GetTypeDescriptor() => items.LastOrDefault()?.TypeDescriptor;

    public object GetValue(object rootObject) {
        if (!TryGetValue(rootObject, out var result)) {
            throw new InvalidOperationException(
                "Unable to retrieve the value of this member path on this root object."
            );
        }

        return result;
    }

    /// <summary>
    ///     Gets the value from the specified root object following this instance path.
    /// </summary>
    /// <param name="rootObject">The root object.</param>
    /// <param name="value">The returned value.</param>
    /// <returns><c>true</c> if evaluation of the path succeeded and the value is valid, <c>false</c> otherwise.</returns>
    /// <exception cref="System.ArgumentNullException">rootObject</exception>
    public bool TryGetValue(object rootObject, out object? value) {
        if (rootObject == null) {
            throw new ArgumentNullException(nameof(rootObject));
        }

        try {
            value = items.Aggregate(rootObject, (current, item) => item.GetValue(current));
        } catch (Exception) {
            // If an exception occurred, we cannot resolve this member path to a valid property/field
            value = null;
            return false;
        }

        return true;
    }

    ///// <summary>
    ///// Gets the value from the specified root object following this instance path.
    ///// </summary>
    ///// <param name="rootObject">The root object.</param>
    ///// <param name="value">The returned value.</param>
    ///// <param name="overrideType">Type of the override.</param>
    ///// <returns><c>true</c> if evaluation of the path succeeded and the value is valid, <c>false</c> otherwise.</returns>
    ///// <exception cref="System.ArgumentNullException">rootObject</exception>
    //public bool TryGetValue(object rootObject, out object value, out OverrideType overrideType)
    //{
    //    if (rootObject == null) throw new ArgumentNullException("rootObject");
    //    if (items.Count == 0) throw new InvalidOperationException("No items pushed via Push methods");

    //    value = null;
    //    overrideType = OverrideType.Base;
    //    try
    //    {
    //        object nextObject = rootObject;

    //        var lastItem = items[items.Count - 1];
    //        var memberDescriptor = lastItem.MemberDescriptor;

    //        for (int i = 0; i < items.Count - 1; i++)
    //        {
    //            var item = items[i];
    //            nextObject = item.GetValue(nextObject);
    //        }

    //        overrideType = nextObject.GetOverride(memberDescriptor);
    //        value = lastItem.GetValue(nextObject);

    //    }
    //    catch (Exception)
    //    {
    //        // If an exception occurred, we cannot resolve this member path to a valid property/field
    //        return false;
    //    }
    //    return true;
    //}

    public IReadOnlyList<MemberPathItem> Decompose() => items;

    /// <summary>
    ///     Clones this instance, cloning the current path.
    /// </summary>
    /// <returns>A clone of this instance.</returns>
    public MemberPath Clone() => new(items);

    /// <summary>
    ///     Clones the inner part of the current path, skipping the given amount of nodes.
    /// </summary>
    /// <param name="containerNodeCount">The number of nodes to skip.</param>
    /// <returns>A clone of this instance.</returns>
    public MemberPath CloneNestedPath(int containerNodeCount) {
        if (containerNodeCount < 0 || containerNodeCount >= items.Count) {
            throw new ArgumentOutOfRangeException(nameof(containerNodeCount));
        }

        return new(items.Skip(containerNodeCount).ToList());
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString() {
        var text = new StringBuilder();
        var isFirst = true;
        foreach (var memberPathItem in items) {
            text.Append(memberPathItem.GetName(isFirst));
            isFirst = false;
        }

        return text.ToString();
    }

    void AddItem(MemberPathItem item) {
        var previousItem = items.Count > 0 ? items[^1] : null;
        items.Add(item);
        item.Parent = previousItem;
    }

    // TODO: improve API for these classes (public part/private part, switch to interfaces)
    public abstract class MemberPathItem {
        public MemberPathItem Parent { get; set; }

        public abstract IMemberDescriptor MemberDescriptor { get; }

        public virtual ITypeDescriptor TypeDescriptor => MemberDescriptor.TypeDescriptor;

        public abstract object GetValue(object thisObj);

        public abstract void SetValue(List<object> stack, int objectIndex, object thisObject, object value);

        public virtual object GetIndex() => null;

        public abstract string GetName(bool isFirst);

        public abstract MemberPathItem Clone(MemberPathItem parent);
    }

    public sealed class PropertyPathItem : MemberPathItem, IEquatable<PropertyPathItem> {
        readonly PropertyDescriptor descriptor;
        readonly bool isValueType;

        public override IMemberDescriptor MemberDescriptor => descriptor;

        public PropertyPathItem(PropertyDescriptor descriptor) {
            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            this.descriptor = descriptor;
            isValueType = descriptor.DeclaringType.IsValueType;
        }

        public override object GetValue(object thisObj) => descriptor.Get(thisObj);

        public override void SetValue(List<object> stack, int objectIndex, object thisObject, object value) {
            descriptor.Set(thisObject, value);

            if (isValueType) {
                Parent?.SetValue(stack, objectIndex - 1, stack[objectIndex - 1], thisObject);
            }
        }

        public override string GetName(bool isFirst) => isFirst ? descriptor.Name : "." + descriptor.Name;

        public override MemberPathItem Clone(MemberPathItem parent) =>
            new PropertyPathItem(descriptor) { Parent = parent };

        public bool Equals(PropertyPathItem other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(descriptor, other.descriptor) && isValueType == other.isValueType;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is PropertyPathItem item && Equals(item);
        }

        public override int GetHashCode() {
            unchecked {
                return (descriptor.GetHashCode() * 397) ^ isValueType.GetHashCode();
            }
        }
    }

    public sealed class FieldPathItem : MemberPathItem, IEquatable<FieldPathItem> {
        readonly FieldDescriptor descriptor;
        readonly bool isValueType;

        public override IMemberDescriptor MemberDescriptor => descriptor;

        public FieldPathItem(FieldDescriptor descriptor) {
            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            this.descriptor = descriptor;
            isValueType = descriptor.DeclaringType.IsValueType;
        }

        public override object GetValue(object thisObj) => descriptor.Get(thisObj);

        public override void SetValue(List<object> stack, int objectIndex, object thisObject, object value) {
            descriptor.Set(thisObject, value);

            if (isValueType) {
                Parent?.SetValue(stack, objectIndex - 1, stack[objectIndex - 1], thisObject);
            }
        }

        public override string GetName(bool isFirst) => "." + descriptor.Name;

        public override MemberPathItem Clone(MemberPathItem parent) =>
            new FieldPathItem(descriptor) { Parent = parent };

        public bool Equals(FieldPathItem other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(descriptor, other.descriptor) && isValueType == other.isValueType;
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is FieldPathItem item && Equals(item);
        }

        public override int GetHashCode() {
            unchecked {
                return (descriptor.GetHashCode() * 397) ^ isValueType.GetHashCode();
            }
        }
    }

    public abstract class SpecialMemberPathItemBase : MemberPathItem {
        public override IMemberDescriptor MemberDescriptor => null;
    }

    public sealed class ArrayPathItem : SpecialMemberPathItemBase, IEquatable<ArrayPathItem> {
        public readonly ArrayDescriptor Descriptor;
        public readonly int Index;

        public override ITypeDescriptor TypeDescriptor => Descriptor;

        public ArrayPathItem(ArrayDescriptor descriptor, int index) {
            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Index = index;
            Descriptor = descriptor;
        }

        public override object GetValue(object thisObj) => ((Array)thisObj).GetValue(Index);

        public override void SetValue(List<object> stack, int objectIndex, object thisObject, object value) {
            ((Array)thisObject).SetValue(value, Index);
        }

        public override string GetName(bool isFirst) => "[" + Index + "]";

        public override object GetIndex() => Index;

        public override MemberPathItem Clone(MemberPathItem parent) =>
            new ArrayPathItem(Descriptor, Index) { Parent = parent };

        public bool Equals(ArrayPathItem other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(Descriptor, other.Descriptor) && Index == other.Index;
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is ArrayPathItem && Equals((ArrayPathItem)obj);
        }

        public override int GetHashCode() => (Descriptor.GetHashCode() * 397) ^ Index;
    }

    public sealed class CollectionPathItem : SpecialMemberPathItemBase, IEquatable<CollectionPathItem> {
        public readonly CollectionDescriptor Descriptor;
        public readonly int Index;

        public override ITypeDescriptor TypeDescriptor => Descriptor;

        public CollectionPathItem(CollectionDescriptor descriptor, int index) {
            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Descriptor = descriptor;
            Index = index;
        }

        public override object GetValue(object thisObj) => Descriptor.GetValue(thisObj, Index);

        public override void SetValue(List<object> stack, int objectIndex, object thisObject, object value) {
            Descriptor.SetValue(thisObject, Index, value);
        }

        public override string GetName(bool isFirst) => "[" + Index + "]";

        public override object GetIndex() => Index;

        public override MemberPathItem Clone(MemberPathItem parent) =>
            new CollectionPathItem(Descriptor, Index) { Parent = parent };

        public bool Equals(CollectionPathItem other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(Descriptor, other.Descriptor) && Index == other.Index;
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is CollectionPathItem item && Equals(item);
        }

        public override int GetHashCode() {
            unchecked {
                return (Descriptor.GetHashCode() * 397) ^ Index;
            }
        }
    }

    public sealed class DictionaryPathItem : SpecialMemberPathItemBase, IEquatable<DictionaryPathItem> {
        public readonly DictionaryDescriptor Descriptor;
        public readonly object Key;

        public override ITypeDescriptor TypeDescriptor => Descriptor;

        public DictionaryPathItem(DictionaryDescriptor descriptor, object key) {
            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Descriptor = descriptor;
            Key = key;
        }

        public override object GetValue(object thisObj) {
            if (!Descriptor.ContainsKey(thisObj, Key)) {
                throw new KeyNotFoundException();
            }

            return Descriptor.GetValue(thisObj, Key);
        }

        public override void SetValue(List<object> stack, int objectIndex, object thisObject, object value) {
            Descriptor.SetValue(thisObject, Key, value);
        }

        public override string GetName(bool isFirst) => "[" + Key + "]";

        public override object GetIndex() => Key;

        public override MemberPathItem Clone(MemberPathItem parent) =>
            new DictionaryPathItem(Descriptor, Key) { Parent = parent };

        public bool Equals(DictionaryPathItem other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(Descriptor, other.Descriptor) && Equals(Key, other.Key);
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is DictionaryPathItem item && Equals(item);
        }

        public override int GetHashCode() {
            unchecked {
                return (Descriptor.GetHashCode() * 397) ^ (Key?.GetHashCode() ?? 0);
            }
        }
    }

    public sealed class SetPathItem : SpecialMemberPathItemBase, IEquatable<SetPathItem> {
        public readonly SetDescriptor Descriptor;
        public readonly object Index;

        public override ITypeDescriptor TypeDescriptor => Descriptor;

        public SetPathItem(SetDescriptor descriptor, object index) {
            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Descriptor = descriptor;
            Index = index;
        }

        public override object GetValue(object thisObj) => Descriptor.GetValue(thisObj, Index);

        public override void SetValue(List<object> stack, int objectIndex, object thisObject, object value) {
            Descriptor.SetValue(thisObject, Index, value);
        }

        public override string GetName(bool isFirst) => "[" + Index + "]";

        public override object GetIndex() => Index;

        public override MemberPathItem Clone(MemberPathItem parent) =>
            new SetPathItem(Descriptor, Index) { Parent = parent };

        public bool Equals(SetPathItem other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(Descriptor, other.Descriptor) && Index == other.Index;
        }

        public override bool Equals(object? obj) => obj is SetPathItem spi && Equals(spi);

        public override int GetHashCode() {
            unchecked {
                return (Descriptor.GetHashCode() * 397) ^ (Index?.GetHashCode() ?? 0);
            }
        }
    }
}
