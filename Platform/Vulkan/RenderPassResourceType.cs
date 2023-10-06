namespace Rin.Platform.Vulkan;

public enum RenderPassResourceType : ushort {
    None = 0, // TODO: remove none?
    UniformBuffer,
    UniformBufferSet,
    StorageBuffer,
    StorageBufferSet,
    Texture2D,
    TextureCube,
    Image2D
}
