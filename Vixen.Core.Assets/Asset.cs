using System.ComponentModel;
using Vixen.Core.Annotations;
using Vixen.Core.Design;
using Vixen.Core.Design.IO;
using Vixen.Core.Reflection;
using Vixen.Core.Serialization.Assets;

namespace Vixen.Core.Assets;

/// <summary>
///     Base class for Asset
/// </summary>
[DataContract(Inherited = true)]
[AssemblyScan]
public abstract class Asset {
    AssetId id;

    // Note: Please keep this code in sync with Package class
    /// <summary>
    ///     Locks the unique identifier for further changes.
    /// </summary>
    internal bool IsIdLocked;

    /// <summary>
    ///     Gets or sets the unique identifier of this asset.
    /// </summary>
    /// <value>The identifier.</value>
    /// <exception cref="System.InvalidOperationException">Cannot change an Asset Object Id once it is locked</exception>
    [DataMember(-10000)]
    [NonOverridable]
    [Display(Browsable = false)]
    public AssetId Id {
        // Note: Please keep this code in sync with Package class
        get => id;
        set {
            if (value != id && IsIdLocked) {
                throw new InvalidOperationException("Cannot change an Asset Object Id once it is locked by a package");
            }

            id = value;
        }
    }

    // Note: Please keep this code in sync with Package class
    /// <summary>
    ///     Gets or sets the version number for this asset, used internally when migrating assets.
    /// </summary>
    /// <value>The version.</value>
    [DataMember(-8000, DataMemberMode.Assign)]
    [DataStyle(DataStyle.Compact)]
    [Display(Browsable = false)]
    [DefaultValue(null)]
    [NonOverridable]
    [NonIdentifiableCollectionItems]
    public Dictionary<string, PackageVersion> SerializedVersion { get; set; }

    /// <summary>
    ///     Gets the tags for this asset.
    /// </summary>
    /// <value>
    ///     The tags for this asset.
    /// </value>
    [DataMember(-1000)]
    [Display(Browsable = false)]
    [NonIdentifiableCollectionItems]
    [NonOverridable]
    [MemberCollection(NotNullItems = true)]
    public TagCollection Tags { get; private set; }

    [DataMember(-500)]
    [Display(Browsable = false)]
    [NonOverridable]
    [DefaultValue(null)]
    public AssetReference Archetype { get; set; }

    /// <summary>
    ///     Gets the main source file for this asset, used in the editor.
    /// </summary>
    [DataMemberIgnore]
    public virtual UFile? MainSource => null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Asset" /> class.
    /// </summary>
    protected Asset() {
        Id = AssetId.NewId();
        Tags = [];

        // Initialize asset with default versions (same code as in Package..ctor())
        var defaultPackageVersion = AssetRegistry.GetCurrentFormatVersions(GetType());
        if (defaultPackageVersion != null) {
            SerializedVersion = new Dictionary<string, PackageVersion>(defaultPackageVersion);
        }
    }

    /// <summary>
    ///     Creates an asset that inherits from this asset.
    /// </summary>
    /// <param name="baseLocation">The location of this asset.</param>
    /// <returns>An asset that inherits this asset instance</returns>
    // TODO: turn internal protected and expose only AssetItem.CreateDerivedAsset()
    public Asset CreateDerivedAsset(string baseLocation) {
        Dictionary<Guid, Guid> idRemapping;
        return CreateDerivedAsset(baseLocation, out idRemapping);
    }

    /// <summary>
    ///     Creates an asset that inherits from this asset.
    /// </summary>
    /// <param name="baseLocation">The location of this asset.</param>
    /// <param name="idRemapping">
    ///     A dictionary in which will be stored all the <see cref="Guid" /> remapping done for the child
    ///     asset.
    /// </param>
    /// <returns>An asset that inherits this asset instance</returns>
    // TODO: turn internal protected and expose only AssetItem.CreateDerivedAsset()
    public virtual Asset CreateDerivedAsset(string baseLocation, out Dictionary<Guid, Guid> idRemapping) {
        if (baseLocation == null) {
            throw new ArgumentNullException(nameof(baseLocation));
        }

        // Make sure we have identifiers for all items
        AssetCollectionItemIdHelper.GenerateMissingItemIds(this);

        // Clone this asset without overrides (as we want all parameters to inherit from base)
        var newAsset = AssetCloner.Clone(this, AssetClonerFlags.GenerateNewIdsForIdentifiableObjects, out idRemapping);

        // Create a new identifier for this asset
        var newId = AssetId.NewId();

        // Register this new identifier in the remapping dictionary
        idRemapping?.Add((Guid)newAsset.Id, (Guid)newId);

        // Write the new id into the new asset.
        newAsset.Id = newId;

        // Create the base of this asset
        newAsset.Archetype = new AssetReference(Id, baseLocation);
        return newAsset;
    }

    public override string ToString() => $"{GetType().Name}: {Id}";
}
