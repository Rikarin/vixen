using Rin.Core.Assets;

namespace Rin.Core;

// General class for loading assets
public abstract class AssetManager {
    Dictionary<AssetId, object> loadedAssets = new();


    public abstract void LoadAsset(AssetId id);
}

// Unique ID per asset (serialized in YAML)

public class AssetSerializer { }

// General importer implementation. Used by both Editor and Runtime
public class AssetImporter { }

public class ModelImporter : AssetImporter { }

public class RuntimeAssetImporter : AssetImporter { }

// Configurable importers used for loading stuff. Move this to Editor only?

public class AssimpMeshImporter : AssetImporter { }
