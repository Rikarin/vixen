using Rin.Platform.Internal;

namespace Rin.Core.General;

public sealed class Shader {
    static readonly Dictionary<string, Shader> shaders = new();

    internal IInternalShader Handle { get; }
    public string Name { get; }
    public string AssetPath { get; } // TODO: move this to Asset class and inherit from it

    internal Shader(IInternalShader handle, string name, string assetPath) {
        Handle = handle;
        Name = name;
        AssetPath = assetPath;
    }

    // Testing purpose
    // public void Bind_Test() => Handle.Bind();

    // public int PropertyToId(string name) => Handle.PropertyToId(name);


    // return shader;
    public static Shader? Find(string name) => shaders.TryGetValue(name, out var shader) ? shader : null;
}
