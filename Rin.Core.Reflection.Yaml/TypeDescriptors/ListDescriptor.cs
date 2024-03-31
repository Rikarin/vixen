using Rin.Core.Reflection.MemberDescriptors;
using System.Collections;
using System.Reflection;

namespace Rin.Core.Reflection.TypeDescriptors;

/// <summary>
///     Provides a descriptor for a <see cref="System.Collections.IList" />.
/// </summary>
public class ListDescriptor : CollectionDescriptor {
    static readonly object[] EmptyObjects = Array.Empty<object>();

    static readonly List<string> ListOfMembersToRemove = [
        "Capacity",
        "Count",
        "IsReadOnly",
        "IsFixedSize",
        "IsSynchronized",
        "SyncRoot",
        "Comparer"
    ];

    readonly Func<object, bool> isReadOnlyFunction;
    readonly Func<object, int> getListCountFunction;
    readonly Func<object, int, object> getIndexedItem;
    readonly Action<object, int, object> setIndexedItem;
    readonly Action<object, object> listAddFunction;
    readonly Action<object, int, object> listInsertFunction;
    readonly Action<object, int> listRemoveAtFunction;
    readonly Action<object, object> listRemoveFunction;
    readonly Action<object> listClearFunction;

    public override DescriptorCategory Category => DescriptorCategory.List;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ListDescriptor" /> class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="type">The type.</param>
    /// <exception cref="System.ArgumentException">Expecting a type inheriting from System.Collections.IList;type</exception>
    public ListDescriptor(
        ITypeDescriptorFactory factory,
        Type type,
        bool emitDefaultValues,
        IMemberNamingConvention namingConvention
    )
        : base(factory, type, emitDefaultValues, namingConvention) {
        if (!IsList(type)) {
            throw new ArgumentException(@"Expecting a type inheriting from System.Collections.IList", nameof(type));
        }

        // Gets the element type
        ElementType = type.GetInterface(typeof(IEnumerable<>))?.GetGenericArguments()[0] ?? typeof(object);

        // implements IList
        if (typeof(IList).IsAssignableFrom(type)) {
            // implements IList
            listAddFunction = (obj, value) => ((IList)obj).Add(value);
            listClearFunction = obj => ((IList)obj).Clear();
            listInsertFunction = (obj, index, value) => ((IList)obj).Insert(index, value);
            listRemoveAtFunction = (obj, index) => ((IList)obj).RemoveAt(index);
            getListCountFunction = o => ((IList)o).Count;
            getIndexedItem = (obj, index) => ((IList)obj)[index];
            setIndexedItem = (obj, index, value) => ((IList)obj)[index] = value;
            isReadOnlyFunction = obj => ((IList)obj).IsReadOnly;
        } else // implements IList<T>
        {
            var add = type.GetMethod(nameof(IList<object>.Add), [ElementType]);
            listAddFunction = (obj, value) => add.Invoke(obj, [value]);
            var remove = type.GetMethod(nameof(IList<object>.Remove), [ElementType]);
            listRemoveFunction = (obj, value) => remove.Invoke(obj, [value]);
            var clear = type.GetMethod(nameof(IList<object>.Clear), Type.EmptyTypes);
            listClearFunction = obj => clear.Invoke(obj, EmptyObjects);
            var countMethod = type.GetProperty(nameof(IList<object>.Count)).GetGetMethod();
            getListCountFunction = o => (int)countMethod.Invoke(o, null);
            var isReadOnly = type.GetInterface(typeof(ICollection<>))
                .GetProperty(nameof(IList<object>.IsReadOnly))
                .GetGetMethod();
            isReadOnlyFunction = obj => (bool)isReadOnly.Invoke(obj, null);
            var insert = type.GetMethod(nameof(IList<object>.Insert), [typeof(int), ElementType]);
            listInsertFunction = (obj, index, value) => insert.Invoke(obj, [index, value]);
            var removeAt = type.GetMethod(nameof(IList<object>.RemoveAt), [typeof(int)]);
            listRemoveAtFunction = (obj, index) => removeAt.Invoke(obj, [index]);
            var getItem = type.GetMethod("get_Item", [typeof(int)]);
            getIndexedItem = (obj, index) => getItem.Invoke(obj, [index]);
            var setItem = type.GetMethod("set_Item", [typeof(int), ElementType]);
            setIndexedItem = (obj, index, value) => setItem.Invoke(obj, [index, value]);
        }

        HasAdd = true;
        HasRemove = true;
        HasInsert = true;
        HasRemoveAt = true;
        HasIndexerAccessors = true;
    }

    public override void Initialize(IComparer<object> keyComparer) {
        base.Initialize(keyComparer);

        IsPureCollection = Count == 0;
    }

    /// <summary>
    ///     Determines whether the specified list is read only.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns><c>true</c> if the specified list is read only; otherwise, <c>false</c>.</returns>
    public override bool IsReadOnly(object list) =>
        list == null || isReadOnlyFunction == null || isReadOnlyFunction(list);

    /// <summary>
    ///     Gets a generic enumerator for a list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>A generic enumerator.</returns>
    /// <exception cref="System.ArgumentNullException">dictionary</exception>
    public IEnumerable<object> GetEnumerator(object list) {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }

        return ((IEnumerable)list).Cast<object>();
    }

    /// <summary>
    ///     Returns the value matching the given index in the list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="index">The index.</param>
    public override object GetValue(object list, object index) {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }

        if (index is not int i) {
            throw new ArgumentException("The index must be an int.");
        }

        return GetValue(list, i);
    }

    /// <summary>
    ///     Returns the value matching the given index in the list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="index">The index.</param>
    public override object GetValue(object list, int index) {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }

        return getIndexedItem(list, index);
    }

    public override void SetValue(object list, object index, object value) {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }

        if (index is not int i) {
            throw new ArgumentException("The index must be an int.");
        }

        SetValue(list, i, value);
    }

    public void SetValue(object list, int index, object value) {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }

        setIndexedItem(list, index, value);
    }

    /// <summary>
    ///     Clears the specified list.
    /// </summary>
    /// <param name="list">The list.</param>
    public override void Clear(object list) {
        listClearFunction(list);
    }

    /// <summary>
    ///     Add to the lists of the same type than this descriptor.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="value">The value to add to this list.</param>
    public override void Add(object list, object value) {
        listAddFunction(list, value);
    }

    /// <summary>
    ///     Insert to the list of the same type than this descriptor.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="index">The index of the insertion.</param>
    /// <param name="value">The value to insert to this list.</param>
    public override void Insert(object list, int index, object value) {
        listInsertFunction(list, index, value);
    }

    /// <summary>
    ///     Removes the item from the lists of the same type.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="item"></param>
    public override void Remove(object list, object item) {
        listRemoveFunction(list, item);
    }

    /// <summary>
    ///     Remove item at the given index from the lists of the same type.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="index">The index of the item to remove from this list.</param>
    public override void RemoveAt(object list, int index) {
        listRemoveAtFunction(list, index);
    }

    /// <summary>
    ///     Determines the number of elements of a list, -1 if it cannot determine the number of elements.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>The number of elements of a list, -1 if it cannot determine the number of elements.</returns>
    public override int GetCollectionCount(object List) =>
        List == null || getListCountFunction == null ? -1 : getListCountFunction(List);

    /// <summary>
    ///     Determines whether the specified type is a .NET list.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if the specified type is list; otherwise, <c>false</c>.</returns>
    public static bool IsList(Type type) {
        if (type == null) {
            throw new ArgumentNullException(nameof(type));
        }

        var typeInfo = type.GetTypeInfo();
        if (typeInfo.IsArray) {
            return false;
        }

        if (typeof(IList).GetTypeInfo().IsAssignableFrom(typeInfo)) {
            return true;
        }

        foreach (var iType in typeInfo.ImplementedInterfaces) {
            var iTypeInfo = iType.GetTypeInfo();
            if (iTypeInfo.IsGenericType && iTypeInfo.GetGenericTypeDefinition() == typeof(IList<>)) {
                return true;
            }
        }

        return false;
    }

    protected override bool PrepareMember(MemberDescriptorBase member, MemberInfo metadataClassMemberInfo) {
        // Filter members
        if (member is PropertyDescriptor && ListOfMembersToRemove.Contains(member.OriginalName)) {
            return false;
        }

        return !IsCompilerGenerated && base.PrepareMember(member, metadataClassMemberInfo);
    }
}
