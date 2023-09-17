namespace Editor.Editor;

public class Metadata {
    public string FullPath { get; }
    public Importer? Importer { get; }

    public Metadata(string fullPath) {
        FullPath = fullPath;
    }
}
