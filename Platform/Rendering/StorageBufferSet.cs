using Rin.Platform.Vulkan;

namespace Rin.Platform.Rendering;

public abstract class StorageBufferSet {
    public abstract StorageBuffer Get();
    public abstract StorageBuffer Get_RT();
    public abstract StorageBuffer Get(int frame);
    public abstract void Set(StorageBuffer storageBuffer, int frame);
    public abstract void Resize(int newSize);

    public static StorageBufferSet Create(StorageBufferOptions options, int size, int framesInFlight = 0) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanStorageBufferSet(options, size, framesInFlight);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
