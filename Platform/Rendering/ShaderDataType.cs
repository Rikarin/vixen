namespace Rin.Platform.Rendering;

public enum ShaderDataType {
    None = 0,
    Float,
    Float2,
    Float3,
    Float4,
    Mat3,
    Mat4,
    Int,
    Int2,
    Int3,
    Int4,
    Bool
}

static class ShaderDataTypeExtensions {
    public static uint Size(this ShaderDataType type) =>
        type switch {
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
}
