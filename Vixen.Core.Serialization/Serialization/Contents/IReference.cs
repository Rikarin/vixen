using Vixen.Core.Assets;

namespace Vixen.Core.Serialization.Contents;

/// <summary>
///     An interface that provides a reference to an object identified by a <see cref="Guid" /> and a location.
/// </summary>
public interface IReference {
    /// <summary>
    ///     Gets the asset unique identifier.
    /// </summary>
    /// <value>The identifier.</value>
    AssetId Id { get; }

    /// <summary>
    ///     Gets the location.
    /// </summary>
    /// <value>The location.</value>
    string Location { get; }
}