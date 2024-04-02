using Arch.Persistence;
using Serilog;
using System.Text;

namespace Vixen.Core.General;

// TODO: finish this
public static class SceneManager {
    static ArchJsonSerializer serializer;
    static Dictionary<string, Scene> loadedScenes;

    public static Scene? ActiveScene { get; private set; }

    public static void Initialize() {
        serializer = new();
    }

    public static Scene CreateScene(string name) {
        var scene = new Scene(name, string.Empty);
        // TODO: what if multiple scenes have the same name?
        // Also we can create scene object on the disk without loading it (I think)
        // loadedScenes[name] = scene;

        return scene;
    }

    public static void SetActiveScene(Scene? scene) {
        if (scene != null) {
            // TODO: not sure about this as we need to reuse the same instance??
            // ActiveScene?.Systems.Dispose();

            scene.Systems.Initialize();
            ActiveScene = scene;
        }
    }

    public static Scene? LoadScene(string path) {
        if (File.Exists(path)) {
            try {
                var json = File.ReadAllText(path, Encoding.UTF8);
                var world = serializer.FromJson(json);
                return new("foo bar", path, world);
            } catch {
                Log.Information("Unable to load Scene");
            }
        }
    
        return null;
    }
    
    public static void SaveScene(Scene scene) {
        scene.World.TrimExcess();
        var json = serializer.ToJson(scene.World);
        File.WriteAllText("Scene01.json", json, Encoding.UTF8);
    }
}
