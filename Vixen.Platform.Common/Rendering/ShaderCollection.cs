namespace Vixen.Platform.Common.Rendering;

// TODO: move this to compiler library
public class ShaderCollection : Dictionary<ShaderStage, ReadOnlyMemory<byte>> { }

public enum ShaderStage {
    Vertex,
    Fragment,
    Compute
}