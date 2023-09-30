using Rin.Core.Abstractions;
using Rin.Platform.Rendering;

namespace Rin.Platform.Vulkan;

public sealed class VulkanUniformBufferSet : UniformBufferSet {
    readonly List<UniformBuffer> uniformBuffers;

    public VulkanUniformBufferSet(int size, int framesInFlight) {
        if (framesInFlight == 0) {
            framesInFlight = Renderer.Options.FramesInFlight;
        }

        uniformBuffers = new(framesInFlight);
        for (var i = 0; i < framesInFlight; i++) {
            uniformBuffers.Add(UniformBuffer.Create(size));
        }
    }

    public override UniformBuffer Get() => Get(Renderer.CurrentFrameIndex);
    public override UniformBuffer Get_RT() => Get(Renderer.CurrentFrameIndex_RT);
    public override UniformBuffer Get(int frame) => uniformBuffers[frame];

    public override void Set(UniformBuffer uniformBuffer, int frame) {
        uniformBuffers[frame] = uniformBuffer;
    }
}