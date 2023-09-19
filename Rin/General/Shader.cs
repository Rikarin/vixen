using Editor.Platform.Internal;

namespace Editor.General;

public class Shader {
    internal IInternalShader Handle { get; }

    public int PropertyToId(string name) => Handle.PropertyToId(name);

    /// <summary>
    ///     Called from Material
    /// </summary>
    internal void Render() {
        // TODO: setup internal stuff
    }
}
