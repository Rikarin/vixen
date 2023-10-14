using Rin.Platform.Abstractions.Rendering;

namespace Rin.Platform.Vulkan;

sealed class RenderPassInput {
    public RenderPassResourceType Type { get; set; }
    public Dictionary<int, object> Input { get; } = new();

    public RenderPassInput() { }

    public RenderPassInput(IUniformBuffer uniformBuffer) {
        Type = RenderPassResourceType.UniformBuffer;
        Input.Add(0, uniformBuffer);
    }

    public RenderPassInput(IUniformBufferSet uniformBufferSet) {
        Type = RenderPassResourceType.UniformBufferSet;
        Input.Add(0, uniformBufferSet);
    }

    public RenderPassInput(IStorageBuffer storageBuffer) {
        Type = RenderPassResourceType.StorageBuffer;
        Input.Add(0, storageBuffer);
    }

    public RenderPassInput(IStorageBufferSet storageBufferSet) {
        Type = RenderPassResourceType.StorageBufferSet;
        Input.Add(0, storageBufferSet);
    }
    
    public RenderPassInput(ITexture2D texture2D) {
        Type = RenderPassResourceType.Texture2D;
        Input.Add(0, texture2D);
    }
    
    public RenderPassInput(ITextureCube textureCube) {
        Type = RenderPassResourceType.TextureCube;
        Input.Add(0, textureCube);
    }
    
    public RenderPassInput(IImage2D image) {
        Type = RenderPassResourceType.Image2D;
        Input.Add(0, image);
    }

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
    
    public void Set(ITexture2D texture, int index = 0) {
        Type = RenderPassResourceType.Texture2D;
        Input[index] = texture;
    }
    
    public void Set(ITextureCube textureCube, int index = 0) {
        Type = RenderPassResourceType.TextureCube;
        Input[index] = textureCube;
    }
    
    public void Set(IImage2D image, int index = 0) {
        Type = RenderPassResourceType.Image2D;
        Input[index] = image;
    }
    
    public void Set(IImageView imageView, int index = 0) {
        Type = RenderPassResourceType.Image2D;
        Input[index] = imageView;
    }
}