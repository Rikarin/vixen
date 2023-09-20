using Rin.Platform.Internal;
using Rin.Platform.Silk;

namespace Rin.Core.General;

public class Shader {
    static Dictionary<string, Shader> loadedShaders = new();
    
    internal IInternalShader Handle { get; }

    Shader(IInternalShader handle) {
        Handle = handle;
    }

    // Testing purpose
    public void Bind_Test() => Handle.Bind();

    public int PropertyToId(string name) => Handle.PropertyToId(name);

    // TODO: This method is used for testing purpose only! Replace with Silk.SPIR-V compiler in Editor and load IL runtime
    public static Shader Create(string name, string vertexPath, string fragmentPath) {
        var shader = new Shader(new OpenGLShader(vertexPath, fragmentPath));

        loadedShaders[name] = shader;
        return shader;
    }

    public static Shader? Find(string name) {
        return loadedShaders.TryGetValue(name, out var shader) ? shader : null;
    }
}
