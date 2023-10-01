namespace Rin.Platform.Vulkan;

public sealed class RenderPassInput {
    public RenderPassResourceType Type { get; }
}

public enum RenderPassResourceType : ushort {
    None = 0,
    UniformBuffer,
    UniformBufferSet,
    StorageBuffer,
    StorageBufferSet,
    Texture2D,
    TextureCube,
    Image2D
}
