namespace Rin.Core.Assets;

public struct AssetId : IEquatable<AssetId>, IComparable<AssetId> {
    public static readonly AssetId Empty = new(Guid.Empty);
    Guid Id;

    public AssetId() { }

    AssetId(Guid id) {
        Id = id;
    }

    public int CompareTo(AssetId other) => Id.CompareTo(other.Id);
    public bool Equals(AssetId other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is AssetId other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Id.ToString();

    public static AssetId NewId() => new(Guid.NewGuid());

    public static explicit operator AssetId(Guid id) => new(id);
    public static explicit operator Guid(AssetId id) => id.Id;

    public static bool operator ==(AssetId left, AssetId right) => left.Equals(right);
    public static bool operator !=(AssetId left, AssetId right) => !left.Equals(right);
}
