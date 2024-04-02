using Vixen.Platform.Common.Rendering;
using Vixen.Platform.Internal;
using Vixen.Rendering;

namespace Vixen.Platform.Vulkan;

sealed class VulkanUniformBufferSet : IUniformBufferSet, IVulkanBufferSet {
    readonly List<IUniformBuffer> uniformBuffers;

    public VulkanUniformBufferSet(int size, int framesInFlight) {
        if (framesInFlight == 0) {
            framesInFlight = Renderer.Options.FramesInFlight;
        }

        uniformBuffers = new(framesInFlight);
        for (var i = 0; i < framesInFlight; i++) {
            uniformBuffers.Add(ObjectFactory.CreateUniformBuffer(size));
        }
    }

    public IUniformBuffer Get() => Get(Renderer.CurrentFrameIndex);
    public IUniformBuffer Get_RT() => Get(Renderer.CurrentFrameIndex_RT);
    public IUniformBuffer Get(int frame) => uniformBuffers[frame];
    public IVulkanBuffer GetVulkanBuffer(int frame) => (IVulkanBuffer)uniformBuffers[frame];

    public void Set(IUniformBuffer uniformBuffer, int frame) {
        uniformBuffers[frame] = uniformBuffer;
    }

}
