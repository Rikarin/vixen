namespace Rin.Core.Abstractions.Shaders;

public record ShaderUniform(string Name, ShaderUniformType Type, int Size, int Offset);
