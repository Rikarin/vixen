using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan.Allocator.Defragmentation;

class BlockListDefragmentationContext {
    public Result Result;
    public bool MutexLocked;

    public readonly List<BlockDefragmentationContext> blockContexts = new();
    public readonly List<DefragmentationMove> DefragMoves = new();

    public int DefragMovesProcessed, DefragMovedCommitted;
    public bool HasDefragmentationPlanned;

    public VulkanMemoryPool? CustomPool { get; }

    public BlockList BlockList { get; }

    public DefragmentationAlgorithm Algorithm { get; }


    public BlockListDefragmentationContext(
        VulkanMemoryAllocator allocator,
        VulkanMemoryPool? customPool,
        BlockList list,
        uint currentFrame
    ) { }
}
