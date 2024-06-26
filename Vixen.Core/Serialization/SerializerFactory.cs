using Vixen.Core.Storage;

namespace Vixen.Core.Serialization;

/// <summary>
///     Used as a fallback when <see cref="SerializerSelector.GetSerializer" /> didn't find anything.
/// </summary>
public abstract class SerializerFactory {
    public abstract DataSerializer GetSerializer(SerializerSelector selector, ref ObjectId typeId);
    public abstract DataSerializer GetSerializer(SerializerSelector selector, Type type);
}
