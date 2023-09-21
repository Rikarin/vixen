namespace Rin.Editor;

class Project {
    public string RootDirectory { get; }

    public string[] Layers => new string[32];
    public string[] Tags => new string[32];

    public Project(string rootDirectory) {
        RootDirectory = rootDirectory;
    }
}
