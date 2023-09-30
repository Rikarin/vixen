using Rin.Core.Abstractions;
using Rin.Platform.Rendering;

namespace Rin.Platform.Vulkan;

public sealed class VulkanStorageBufferSet : StorageBufferSet {
    readonly List<StorageBuffer> storageBuffers;

    public VulkanStorageBufferSet(StorageBufferOptions options, int size, int framesInFlight) {
        if (framesInFlight == 0) {
            framesInFlight = Renderer.Options.FramesInFlight;
        }

        storageBuffers = new(framesInFlight);
        for (var i = 0; i < framesInFlight; i++) {
            storageBuffers.Add(StorageBuffer.Create(options, size));
        }
    }

    public override StorageBuffer Get() => Get(Renderer.CurrentFrameIndex);
    public override StorageBuffer Get_RT() => Get(Renderer.CurrentFrameIndex_RT);
    public override StorageBuffer Get(int frame) => storageBuffers[frame];

    public override void Set(StorageBuffer storageBuffer, int frame) {
        storageBuffers[frame] = storageBuffer;
    }

    public override void Resize(int newSize) {
        foreach (var buffer in storageBuffers) {
            buffer.Resize(newSize);
        }
    }
}
