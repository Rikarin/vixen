namespace Rin.Core.Reflection.TypeDescriptors;

/// <summary>
///     A descriptor for an array.
/// </summary>
public class ArrayDescriptor : ObjectDescriptor {
    public override DescriptorCategory Category => DescriptorCategory.Array;

    /// <summary>
    ///     Gets the type of the array element.
    /// </summary>
    /// <value>The type of the element.</value>
    public Type ElementType { get; }

    public ArrayDescriptor(
        ITypeDescriptorFactory factory,
        Type type,
        bool emitDefaultValues,
        IMemberNamingConvention namingConvention
    ) : base(factory, type, emitDefaultValues, namingConvention) {
        if (!type.IsArray) {
            throw new ArgumentException("Expecting array type", nameof(type));
        }

        if (type.GetArrayRank() != 1) {
            throw new ArgumentException(
                $"Cannot support dimension [{type.GetArrayRank()}] for type [{type.FullName}]. Only supporting dimension of 1"
            );
        }

        ElementType = type.GetElementType();
    }

    /// <summary>
    ///     Creates the equivalent of list type for this array.
    /// </summary>
    /// <returns>A list type with same element type than this array.</returns>
    public Array CreateArray(int dimension) => Array.CreateInstance(ElementType, dimension);

    /// <summary>
    ///     Retrieves the item corresponding to the given index in the array.
    /// </summary>
    /// <param name="array">The array in which to read the item.</param>
    /// <param name="index">The index of the item to read.</param>
    /// <returns>The item corresponding to the given index in the array.</returns>
    public object GetValue(object array, int index) => ((Array)array).GetValue(index);

    public void SetValue(object array, int index, object value) {
        ((Array)array).SetValue(value, index);
    }

    /// <summary>
    ///     Determines the number of elements of an array, -1 if it cannot determine the number of elements.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <returns>The number of elements of an array, -1 if it cannot determine the number of elements.</returns>
    public int GetLength(object array) => ((Array)array).Length;
}
