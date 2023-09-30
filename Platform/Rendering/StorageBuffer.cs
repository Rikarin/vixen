using Rin.Platform.Vulkan;

namespace Rin.Platform.Rendering;

public abstract class StorageBuffer : IDisposable {
    public abstract void SetData(ReadOnlySpan<byte> data);
    public abstract void SetData_RT(ReadOnlySpan<byte> data);
    public abstract void Resize(int newSize);
    public abstract void Dispose();

    public static StorageBuffer Create(StorageBufferOptions options, int size) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanStorageBuffer(options, size);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
