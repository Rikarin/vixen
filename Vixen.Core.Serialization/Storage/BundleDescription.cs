namespace Vixen.Core.Storage;

/// <summary>
///     Description of a bundle: header, dependencies, objects and assets.
/// </summary>
public class BundleDescription {
    public BundleOdbBackend.Header Header { get; set; }
    public List<string> Dependencies { get; private set; } = new();
    public List<ObjectId> IncrementalBundles { get; private set; } = new();
    public List<KeyValuePair<ObjectId, BundleOdbBackend.ObjectInfo>> Objects { get; private set; } = new();
    public List<KeyValuePair<string, ObjectId>> Assets { get; private set; } = new();
}
