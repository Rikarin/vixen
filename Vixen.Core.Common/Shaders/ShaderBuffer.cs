namespace Vixen.Core.Common.Shaders;

public sealed class ShaderBuffer {
    public string Name { get; set; }
    public int Size { get; set; }
    public Dictionary<string, ShaderUniform> Uniforms { get; } = new();
}
