using Rin.Platform.Rendering;

namespace Rin.Platform.Vulkan;

public sealed class RenderPassInput {
    public RenderPassResourceType Type { get; private set; }
    public List<object> Input { get; } = new();

    public RenderPassInput() { }

    public RenderPassInput(IUniformBuffer uniformBuffer) {
        Type = RenderPassResourceType.UniformBuffer;
        Input.Add(uniformBuffer);
    }

    public RenderPassInput(IUniformBufferSet uniformBufferSet) {
        Type = RenderPassResourceType.UniformBufferSet;
        Input.Add(uniformBufferSet);
    }

    public RenderPassInput(IStorageBuffer storageBuffer) {
        Type = RenderPassResourceType.StorageBuffer;
        Input.Add(storageBuffer);
    }

    public RenderPassInput(IStorageBufferSet storageBufferSet) {
        Type = RenderPassResourceType.StorageBufferSet;
        Input.Add(storageBufferSet);
    }

    // TODO: Texture2D, TextureCube, Image2D

    public void Set(IUniformBuffer uniformBuffer, int index = 0) {
        Type = RenderPassResourceType.UniformBuffer;
        Input[index] = uniformBuffer;
    }

    public void Set(IUniformBufferSet uniformBufferSet, int index = 0) {
        Type = RenderPassResourceType.UniformBufferSet;
        Input[index] = uniformBufferSet;
    }

    public void Set(IStorageBuffer storageBuffer, int index = 0) {
        Type = RenderPassResourceType.StorageBuffer;
        Input[index] = storageBuffer;
    }

    public void Set(IStorageBufferSet storageBufferSet, int index = 0) {
        Type = RenderPassResourceType.StorageBufferSet;
        Input[index] = storageBufferSet;
    }

    // TODO: Texture2D, TextureCube, Image2D, ImageView
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

public enum RenderPassInputType : ushort {
    None = 0,
    UniformBuffer,
    StorageBuffer,
    ImageSampler1D,
    ImageSampler2D,
    ImageSampler3D, // TODO: 3D vs Cube?
    StorageImage1D,
    StorageImage2D,
    StorageImage3D
}

public record RenderPassInputDeclaration(RenderPassInputType Type, int Set, int Binding, int Count, string Name);
