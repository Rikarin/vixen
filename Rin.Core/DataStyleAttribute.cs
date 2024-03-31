namespace Rin.Core;

/// <summary>
///     An attribute to modify the output style of a sequence or mapping.
///     This attribute can be apply directly on a type or on a property/field.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
public class DataStyleAttribute : Attribute {
    /// <summary>
    ///     Gets the style.
    /// </summary>
    /// <value>The style.</value>
    public DataStyle Style { get; }

    /// <summary>
    ///     Gets the style.
    /// </summary>
    /// <value>The style.</value>
    public ScalarStyle ScalarStyle { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataStyleAttribute" /> class.
    /// </summary>
    /// <param name="style">The style.</param>
    public DataStyleAttribute(DataStyle style) {
        Style = style;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataStyleAttribute" /> class.
    /// </summary>
    /// <param name="scalarStyle">The style.</param>
    public DataStyleAttribute(ScalarStyle scalarStyle) {
        ScalarStyle = scalarStyle;
    }
}