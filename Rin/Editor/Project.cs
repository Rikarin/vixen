namespace Editor.Editor;

public class Project {
    public string RootDirectory { get; }

    public Project(string rootDirectory) {
        RootDirectory = rootDirectory;
    }
}
