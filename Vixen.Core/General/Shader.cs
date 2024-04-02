
using Vixen.Platform.Common.Rendering;

namespace Vixen.Core.General;

public sealed class Shader {
    static readonly Dictionary<string, Shader> shaders = new();
    public string Name { get; }
    public string AssetPath { get; } // TODO: move this to Asset class and inherit from it

    internal IShader Handle { get; }

    internal Shader(IShader handle, string name, string assetPath) {
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
