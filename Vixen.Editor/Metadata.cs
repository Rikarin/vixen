namespace Vixen.Editor;

public class Metadata {
    public string FullPath { get; }
    public AssetImporter? Importer { get; }

    public Metadata(string fullPath) {
        FullPath = fullPath;
    }
}
