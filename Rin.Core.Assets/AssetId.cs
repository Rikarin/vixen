namespace Rin.Core.Assets;

public record struct AssetId : IComparable<AssetId> {
    public static readonly AssetId Empty = new(Guid.Empty);
    Guid Id;

    public AssetId() { }

    AssetId(Guid id) {
        Id = id;
    }

    public int CompareTo(AssetId other) => Id.CompareTo(other.Id);
    public override string ToString() => Id.ToString();

    public static AssetId NewId() => new(Guid.NewGuid());

    public static explicit operator AssetId(Guid id) => new(id);
    public static explicit operator Guid(AssetId id) => id.Id;
}
