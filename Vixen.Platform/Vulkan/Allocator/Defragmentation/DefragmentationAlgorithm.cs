using Silk.NET.Vulkan;

namespace Vixen.Platform.Vulkan.Allocator.Defragmentation;

abstract class DefragmentationAlgorithm : IDisposable {
    protected readonly VulkanMemoryAllocator Allocator;
    protected readonly BlockList BlockList;
    protected readonly uint CurrentFrame;

    public abstract ulong BytesMoved { get; }

    public abstract int AllocationsMoved { get; }

    protected DefragmentationAlgorithm(VulkanMemoryAllocator allocator, BlockList list, uint currentFrame) {
        Allocator = allocator;
        BlockList = list;
        CurrentFrame = currentFrame;
    }

    public virtual void Dispose() { }

    public abstract void AddAllocation(Allocation alloc, out bool changed);

    public abstract void AddAll();

    public abstract Result Defragment(
        ulong maxBytesToMove,
        int maxAllocationsToMove,
        DefragmentationFlags flags,
        out DefragmentationMove[] moves
    );

    protected class AllocateInfo {
        public Allocation Allocation;
        public bool Changed;

        public AllocateInfo() { }

        public AllocateInfo(Allocation allocation) {
            Allocation = allocation;
        }
    }
}
