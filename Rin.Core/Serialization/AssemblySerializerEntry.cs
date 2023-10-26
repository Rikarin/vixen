using Rin.Core.Storage;

namespace Rin.Core.Serialization;

/// <summary>
///     An entry to a serialized object.
/// </summary>
public struct AssemblySerializerEntry {
    /// <summary>
    ///     The id of the object.
    /// </summary>
    public readonly ObjectId Id;

    /// <summary>
    ///     The type of the object.
    /// </summary>
    public readonly Type ObjectType;

    /// <summary>
    ///     The type of the serialized object.
    /// </summary>
    public readonly Type SerializerType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssemblySerializerEntry" /> struct.
    /// </summary>
    public AssemblySerializerEntry(ObjectId id, Type objectType, Type serializerType) {
        Id = id;
        ObjectType = objectType;
        SerializerType = serializerType;
    }
}
