namespace Vixen.Platform.Common.Rendering;

public sealed class VertexBufferElement {
    public string Name { get; }
    public ShaderDataType Type { get; }
    public int Offset { get; internal set; }
    public bool Normalized { get; }

    public int Size =>
        Type switch {
            ShaderDataType.Float => 4,
            ShaderDataType.Float2 => 4 * 2,
            ShaderDataType.Float3 => 4 * 3,
            ShaderDataType.Float4 => 4 * 4,
            ShaderDataType.Mat3 => 4 * 3 * 3,
            ShaderDataType.Mat4 => 4 * 4 * 4,
            ShaderDataType.Int => 4,
            ShaderDataType.Int2 => 4 * 2,
            ShaderDataType.Int3 => 4 * 3,
            ShaderDataType.Int4 => 4 * 4,
            ShaderDataType.Bool => 1,
            _ => throw new ArgumentOutOfRangeException()
        };

    public VertexBufferElement(ShaderDataType type, string name, bool normalized = false) {
        Type = type;
        Name = name;
        Normalized = normalized;
    }
}
