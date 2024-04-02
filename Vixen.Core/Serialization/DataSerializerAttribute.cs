namespace Vixen.Core.Serialization;

/// <summary>
///     Use this attribute on a class to specify its data serializer type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataSerializerAttribute : Attribute {
    /// <summary>
    ///     Gets the type of the data serializer.
    /// </summary>
    /// <value>
    ///     The type of the data serializer.
    /// </value>
    public Type DataSerializerType;

    public DataSerializerGenericMode Mode;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataSerializerAttribute" /> class.
    /// </summary>
    /// <param name="dataSerializerType">Type of the data serializer.</param>
    public DataSerializerAttribute(Type dataSerializerType) {
        DataSerializerType = dataSerializerType;
    }
}