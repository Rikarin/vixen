using Rin.Platform.Internal;

namespace Rin.Core.General;

public sealed class Shader {
    static readonly Dictionary<string, Shader> shaders = new();

    internal IInternalShader Handle { get; }

    Shader(IInternalShader handle) {
        Handle = handle;
    }

    // Testing purpose
    // public void Bind_Test() => Handle.Bind();

    // public int PropertyToId(string name) => Handle.PropertyToId(name);

    // TODO: This method is used for testing purpose only! Replace with Silk.SPIR-V compiler in Editor and load IL runtime
    public static Shader Create(string name, string vertexPath, string fragmentPath) =>
        throw
            // var shader = new Shader(new OpenGLShader(vertexPath, fragmentPath));
            // loadedShaders[name] = shader;
            new NotImplementedException();

    // return shader;
    public static Shader? Find(string name) => shaders.TryGetValue(name, out var shader) ? shader : null;
}
