namespace Rin.Platform.Rendering;

public sealed class VertexBufferElement {
    public string Name { get; }
    public ShaderDataType Type { get; }
    public uint Size { get; }
    public uint Offset { get; internal set; }
    public bool Normalized { get; }

    public int ComponentCount =>
        Type switch {
            ShaderDataType.Float => 1,
            ShaderDataType.Float2 => 2,
            ShaderDataType.Float3 => 3,
            ShaderDataType.Float4 => 4,
            ShaderDataType.Mat3 => 3 * 3, // 3 * float3
            ShaderDataType.Mat4 => 4 * 4, // 4 * float4
            ShaderDataType.Int => 1,
            ShaderDataType.Int2 => 2,
            ShaderDataType.Int3 => 3,
            ShaderDataType.Int4 => 4,
            ShaderDataType.Bool => 1,
            _ => throw new ArgumentOutOfRangeException()
        };


    public VertexBufferElement(ShaderDataType type, string name, bool normalized = false) {
        Type = type;
        Name = name;
        Normalized = normalized;
    }
}
