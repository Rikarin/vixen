namespace Vixen.Core;

/// <summary>
///     Specifies the metadata class to associate with a serializable class.
///     The main usage of this class is to allow a sub-class to override property
///     attributes such as <see cref="System.ComponentModel.DefaultValueAttribute" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DataContractMetadataTypeAttribute : Attribute {
    /// <summary>
    ///     Gets the metadata class that is associated with a serializable class.
    /// </summary>
    public Type MetadataClassType { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataContractMetadataTypeAttribute" /> class.
    /// </summary>
    /// <param name="metadataClassType">The type alias name when serializing to a textual format.</param>
    /// <exception cref="ArgumentException"><paramref name="metadataClassType" /> is <c>null</c></exception>
    public DataContractMetadataTypeAttribute(Type metadataClassType) {
        MetadataClassType = metadataClassType;
    }
}
