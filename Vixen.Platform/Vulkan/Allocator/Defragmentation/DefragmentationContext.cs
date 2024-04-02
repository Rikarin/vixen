using Silk.NET.Vulkan;

namespace Vixen.Platform.Vulkan.Allocator.Defragmentation;

public sealed class DefragmentationContext : IDisposable {
    readonly VulkanMemoryAllocator Allocator;
    readonly uint currentFrame;
    readonly uint Flags;
    DefragmentationStats Stats;

    ulong MaxCPUBytesToMove, MaxGPUBytesToMove;
    int MaxCPUAllocationsToMove, MaxGPUAllocationsToMove;

    readonly BlockListDefragmentationContext[] DefaultPoolContexts =
        new BlockListDefragmentationContext[Vk.MaxMemoryTypes];

    readonly List<BlockListDefragmentationContext> CustomPoolContexts = new();


    internal DefragmentationContext(
        VulkanMemoryAllocator allocator,
        uint currentFrame,
        uint flags,
        DefragmentationStats stats
    ) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        throw new NotImplementedException();
    }

    internal void AddPools(params VulkanMemoryPool[] Pools) {
        throw new NotImplementedException();
    }

    internal void AddAllocations(Allocation[] allocations, out bool[] allocationsChanged) {
        throw new NotImplementedException();
    }

    internal Result Defragment(
        ulong maxCPUBytesToMove,
        int maxCPUAllocationsToMove,
        ulong maxGPUBytesToMove,
        int maxGPUAllocationsToMove,
        CommandBuffer cbuffer,
        DefragmentationStats stats,
        DefragmentationFlags flags
    ) =>
        throw new NotImplementedException();

    internal Result DefragmentationPassBegin(ref DefragmentationPassMoveInfo[] Info) =>
        throw new NotImplementedException();

    internal Result DefragmentationPassEnd() => throw new NotImplementedException();
}
