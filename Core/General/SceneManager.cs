namespace Rin.Core.General;

// TODO: finish this
public static class SceneManager {
    static Dictionary<string, Scene> loadedScenes = new();
    
    public static Scene ActiveScene { get; private set; }
    
    public static Scene CreateScene(string name) {
        var scene = new Scene(name, string.Empty);
        // TODO: what if multiple scenes have the same name?
        loadedScenes[name] = scene;

        return scene;
    }

    static Scene LoadScene(string path) {
        throw new NotImplementedException();
    }

    public static void SetActiveScene(Scene scene) {
        ActiveScene = scene;
    }
}
