namespace Rin.Core;

/// <summary>
///     Allows to re-map a previous class/field/property/enum name to the specified property/field/enum/class/struct.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property
    | AttributeTargets.Field
    | AttributeTargets.Enum
    | AttributeTargets.Class
    | AttributeTargets.Struct,
    AllowMultiple = true
)]
public sealed class DataAliasAttribute : Attribute {
    /// <summary>
    ///     Gets the previous name.
    /// </summary>
    /// <value>The previous name.</value>
    public string Name { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataAliasAttribute" /> class.
    /// </summary>
    /// <param name="name">The previous name.</param>
    public DataAliasAttribute(string name) {
        Name = name;
    }
}
