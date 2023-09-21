namespace Rin.Core.General;

public class Scene {
    List<GameObject> gameObjects = new();
    
    public string Name { get; }
    public bool IsDirty { get; }
    public bool IsLoaded { get; }
    
    /// <summary>
    /// Relative path of the scene. "Assets/Scenes/Scene01.rin"
    /// </summary>
    public string Path { get; }

    IReadOnlyList<GameObject> RootGameObjects => gameObjects.AsReadOnly();

    internal Scene(string name, string path) {
        Name = name;
        Path = path;
    }
}
