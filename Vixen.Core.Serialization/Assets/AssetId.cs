using Vixen.Core.Serialization;

namespace Vixen.Core.Assets;

[DataContract("AssetId")]
[DataSerializer(typeof(Serializer))]
public record struct AssetId : IComparable<AssetId> {
    readonly Guid Id;
    public static readonly AssetId Empty = new(Guid.Empty);

    public AssetId() { }

    AssetId(Guid id) {
        Id = id;
    }

    public int CompareTo(AssetId other) => Id.CompareTo(other.Id);
    public override string ToString() => Id.ToString();

    public static AssetId NewId() => new(Guid.NewGuid());

    public static explicit operator AssetId(Guid id) => new(id);
    public static explicit operator Guid(AssetId id) => id.Id;

    public static bool TryParse(string input, out AssetId result) {
        var success = Guid.TryParse(input, out var guid);
        result = new(guid);
        return success;
    }


    class Serializer : DataSerializer<AssetId> {
        DataSerializer<Guid>? guidSerializer;

        public override void Initialize(SerializerSelector serializerSelector) {
            base.Initialize(serializerSelector);
            guidSerializer = serializerSelector.GetSerializer<Guid>();
        }

        public override void Serialize(ref AssetId obj, ArchiveMode mode, SerializationStream stream) {
            var guid = obj.Id;
            guidSerializer?.Serialize(ref guid, mode, stream);
            if (mode == ArchiveMode.Deserialize) {
                obj = new(guid);
            }
        }
    }
}
