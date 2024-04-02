using Vixen.Core.Design.IO;
using Vixen.Core.Serialization;
using Vixen.Core.Serialization.Contents;

namespace Vixen.Core.Assets;

/// <summary>
///     An asset reference.
/// </summary>
[DataContract("aref")]
[DataStyle(DataStyle.Compact)]
[DataSerializer(typeof(AssetReferenceDataSerializer))]
public sealed class AssetReference : IReference, IEquatable<AssetReference> {
    /// <summary>
    ///     Gets or sets the unique identifier of the reference asset.
    /// </summary>
    /// <value>The unique identifier of the reference asset..</value>
    [DataMember(10)]
    public AssetId Id { get; }

    /// <summary>
    ///     Gets or sets the location of the asset.
    /// </summary>
    /// <value>The location.</value>
    [DataMember(20)]
    public string Location { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetReference" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the asset.</param>
    /// <param name="location">The location.</param>
    public AssetReference(AssetId id, UFile location) {
        Location = location;
        Id = id;
    }

    public bool Equals(AssetReference other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(Location, other.Location) && Id.Equals(other.Id);
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        return Equals(obj as AssetReference);
    }

    public override int GetHashCode() {
        unchecked {
            return ((Location?.GetHashCode() ?? 0) * 397) ^ Id.GetHashCode();
        }
    }

    /// <summary>
    ///     Implements the ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(AssetReference left, AssetReference right) => Equals(left, right);

    /// <summary>
    ///     Implements the !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(AssetReference left, AssetReference right) => !Equals(left, right);

    /// <inheritdoc />
    public override string ToString() =>
        // WARNING: This should not be modified as it is used for serializing
        $"{Id}:{Location}";

    /// <summary>
    ///     Tries to parse an asset reference in the format "GUID:Location".
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="location">The location.</param>
    /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
    public static AssetReference New(AssetId id, UFile location) => new(id, location);

    /// <summary>
    ///     Tries to parse an asset reference in the format "[GUID/]GUID:Location". The first GUID is optional and is used to
    ///     store the ID of the reference.
    /// </summary>
    /// <param name="assetReferenceText">The asset reference.</param>
    /// <param name="id">The unique identifier of asset pointed by this reference.</param>
    /// <param name="location">The location.</param>
    /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
    /// <exception cref="System.ArgumentNullException">assetReferenceText</exception>
    public static bool TryParse(string assetReferenceText, out AssetId id, out UFile? location) {
        if (assetReferenceText == null) {
            throw new ArgumentNullException(nameof(assetReferenceText));
        }

        id = AssetId.Empty;
        location = null;
        var indexFirstSlash = assetReferenceText.IndexOf('/');
        var indexBeforeLocation = assetReferenceText.IndexOf(':');
        if (indexBeforeLocation < 0) {
            return false;
        }

        var startNextGuid = 0;
        if (indexFirstSlash > 0 && indexFirstSlash < indexBeforeLocation) {
            startNextGuid = indexFirstSlash + 1;
        }

        if (!AssetId.TryParse(
                assetReferenceText.Substring(startNextGuid, indexBeforeLocation - startNextGuid),
                out id
            )) {
            return false;
        }

        location = new(assetReferenceText.Substring(indexBeforeLocation + 1));

        return true;
    }

    /// <summary>
    ///     Tries to parse an asset reference in the format "GUID:Location".
    /// </summary>
    /// <param name="assetReferenceText">The asset reference.</param>
    /// <param name="assetReference">The reference.</param>
    /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
    public static bool TryParse(string assetReferenceText, out AssetReference? assetReference) {
        if (assetReferenceText == null) {
            throw new ArgumentNullException(nameof(assetReferenceText));
        }

        assetReference = null;
        if (!TryParse(assetReferenceText, out var assetId, out var location)) {
            return false;
        }

        assetReference = New(assetId, location);
        return true;
    }
}
