namespace Rin.Editor;

class EditorManager {
    readonly Project project;
    FileSystemWatcher watcher;

    public EditorManager(Project project) {
        this.project = project;
    }

    public void Watch() {
        var assetPath = Path.Combine(project.RootDirectory, "Assets");
        watcher = new(assetPath);

        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        watcher.Created += OnCreated;
        watcher.Renamed += OnRenamed;
    }

    void OnRenamed(object sender, RenamedEventArgs e) {
        if (!CanHandleExtension(e.OldFullPath)) {
            return;
        }

        Console.WriteLine($"Renamed file {e.Name} {e.FullPath}");
        MetadataManager.RenameMetadata(e.OldFullPath, e.FullPath);
    }

    void OnCreated(object sender, FileSystemEventArgs e) {
        if (!CanHandleExtension(e.FullPath)) {
            return;
        }

        Console.WriteLine($"Created file {e.Name} {e.FullPath}");
        MetadataManager.CreateMetadata(e.FullPath);
    }

    bool CanHandleExtension(string fullPath) => !fullPath.EndsWith(".meta") && !fullPath.EndsWith(".DS_Store");
}
