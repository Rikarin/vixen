using Vixen.Platform.Common.Rendering;

namespace Vixen.Platform.Vulkan;

sealed class RenderPassInput {
    public RenderPassResourceType Type { get; set; }
    public List<object?> Input { get; } = new();

    public RenderPassInput(int count) {
        for (var i = 0; i < count; i++) {
            Input.Add(null);
        }
    }

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
    
    public RenderPassInput(ITexture2D texture2D) {
        Type = RenderPassResourceType.Texture2D;
        Input.Add(texture2D);
    }
    
    public RenderPassInput(ITextureCube textureCube) {
        Type = RenderPassResourceType.TextureCube;
        Input.Add(textureCube);
    }
    
    public RenderPassInput(IImage2D image) {
        Type = RenderPassResourceType.Image2D;
        Input.Add(image);
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