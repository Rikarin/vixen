using Arch.Core;
using Arch.System;
using Rin.Core.Components;

namespace Rin.Core.General;

public class Scene {
    readonly List<GameObject> gameObjects = new();

    public string Name { get; }
    public bool IsDirty { get; }
    public bool IsLoaded { get; }

    /// <summary>
    ///     Relative path of the scene. "Assets/Scenes/Scene01.rin"
    /// </summary>
    public string Path { get; }

    // ECS
    public World World { get; }
    public Group<float> Systems { get; }

    IReadOnlyList<GameObject> RootGameObjects => gameObjects.AsReadOnly();

    internal Scene(string name, string path) : this(name, path, World.Create()) { }

    internal Scene(string name, string path, World world) {
        Name = name;
        Path = path;
        World = world;

        // TODO: Load this
        Systems = new(
            new ParentSystem(World),
            new LocalToWorldSystem(World)
        );
    }

    public void Save() {
        SceneManager.SaveScene(this);
    }
}
