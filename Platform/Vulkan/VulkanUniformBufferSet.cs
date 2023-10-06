using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Rendering;

namespace Rin.Platform.Vulkan;

public sealed class VulkanUniformBufferSet : IUniformBufferSet {
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

    public void Set(IUniformBuffer uniformBuffer, int frame) {
        uniformBuffers[frame] = uniformBuffer;
    }
}
