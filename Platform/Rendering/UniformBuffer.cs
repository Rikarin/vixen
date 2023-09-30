using Rin.Platform.Vulkan;

namespace Rin.Platform.Rendering;

public abstract class UniformBuffer : IDisposable {
    public abstract void SetData(ReadOnlySpan<byte> data);
    public abstract void SetData_RT(ReadOnlySpan<byte> data);
    public abstract void Dispose();

    public static UniformBuffer Create(int size) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanUniformBuffer(size);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}