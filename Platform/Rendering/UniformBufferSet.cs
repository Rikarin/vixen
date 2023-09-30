using Rin.Platform.Vulkan;

namespace Rin.Platform.Rendering; 

public abstract class UniformBufferSet {
    public abstract UniformBuffer Get();
    public abstract UniformBuffer Get_RT();
    public abstract UniformBuffer Get(int frame);
    public abstract void Set(UniformBuffer uniformBuffer, int frame);

    public static UniformBufferSet Create(int size, int framesInFlight = 0) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanUniformBufferSet(size, framesInFlight);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}