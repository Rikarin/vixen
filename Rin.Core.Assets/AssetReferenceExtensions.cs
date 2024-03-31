namespace Rin.Core.Assets;

/// <summary>
///     Extension methods for <see cref="AssetReference" />
/// </summary>
public static class AssetReferenceExtensions {
    /// <summary>
    ///     Determines whether the specified asset reference has location. If the reference is null, return <c>false</c>.
    /// </summary>
    /// <param name="assetReference">The asset reference.</param>
    /// <returns><c>true</c> if the specified asset reference has location; otherwise, <c>false</c>.</returns>
    public static bool HasLocation(this AssetReference assetReference) =>
        assetReference != null && assetReference.Location != null;
}
