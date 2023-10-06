namespace Rin.Platform.Vulkan;

public enum RenderPassInputType : ushort {
    None = 0, // TODO: remove none??
    UniformBuffer,
    StorageBuffer,
    ImageSampler1D,
    ImageSampler2D,
    ImageSampler3D, // TODO: 3D vs Cube?
    StorageImage1D,
    StorageImage2D,
    StorageImage3D
}
