using Rin.Core.Serialization;
using Rin.Core.Serialization.Assets;

namespace Rin.Core.Assets;

/// <summary>
///     Serializer for <see cref="AssetReference" />.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class AssetReferenceDataSerializer : DataSerializer<AssetReference> {
    /// <inheritdoc />
    public override void Serialize(ref AssetReference? assetReference, ArchiveMode mode, SerializationStream stream) {
        if (mode == ArchiveMode.Serialize) {
            stream.Write(assetReference.Id);
            stream.Write(assetReference.Location);
        } else {
            var id = stream.Read<AssetId>();
            var location = stream.ReadString();

            assetReference = new(id, location);
        }
    }
}
