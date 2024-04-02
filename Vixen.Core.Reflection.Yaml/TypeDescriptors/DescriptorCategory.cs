namespace Vixen.Core.Reflection.TypeDescriptors;

/// <summary>
///     A category used by <see cref="ITypeDescriptorBase" />.
/// </summary>
public enum DescriptorCategory {
    /// <summary>
    ///     A primitive.
    /// </summary>
    Primitive,

    /// <summary>
    ///     A collection.
    /// </summary>
    Collection,

    /// <summary>
    ///     An array
    /// </summary>
    Array,

    /// <summary>
    ///     A list
    /// </summary>
    List,

    /// <summary>
    ///     A dictionary
    /// </summary>
    Dictionary,

    /// <summary>
    ///     A set
    /// </summary>
    Set,

    /// <summary>
    ///     An object
    /// </summary>
    Object,

    /// <summary>
    ///     An unsupported object. This will be treated the same as Object.
    /// </summary>
    NotSupportedObject,

    /// <summary>
    ///     A nullable value
    /// </summary>
    Nullable,

    /// <summary>
    ///     A custom descriptor.
    /// </summary>
    Custom
}
