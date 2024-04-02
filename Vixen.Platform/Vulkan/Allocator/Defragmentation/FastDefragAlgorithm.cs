using Silk.NET.Vulkan;
using Vixen.Platform.Vulkan.Allocator.Metadata;

namespace Vixen.Platform.Vulkan.Allocator.Defragmentation;

sealed class FastDefragAlgorithm : DefragmentationAlgorithm {
    readonly bool overlappingMoveSupported;
    int allocationCount;
    bool allAllocations;

    ulong bytesMoved;
    int allocationsMoved;

    readonly List<BlockInfo> blockInfos = new();

    public override ulong BytesMoved => throw new NotImplementedException();

    public override int AllocationsMoved => throw new NotImplementedException();

    public FastDefragAlgorithm(
        VulkanMemoryAllocator allocator,
        BlockList list,
        uint currentFrame,
        bool overlappingMoveSupported
    ) : base(allocator, list, currentFrame) {
        this.overlappingMoveSupported = overlappingMoveSupported;
    }

    public override void AddAll() {
        throw new NotImplementedException();
    }

    public override void AddAllocation(Allocation alloc, out bool changed) {
        throw new NotImplementedException();
    }

    public override Result Defragment(
        ulong maxBytesToMove,
        int maxAllocationsToMove,
        DefragmentationFlags flags,
        out DefragmentationMove[] moves
    ) =>
        throw new NotImplementedException();

    void PreprocessMetadata() { }

    void PostprocessMetadata() { }

    void InsertSuballoc(BlockMetadata_Generic metadata, in Suballocation suballoc) { }

    struct BlockInfo {
        public int OrigBlockIndex;
    }

    class FreeSpaceDatabase {
        const int MaxCount = 4;

        readonly FreeSpace[] FreeSpaces = new FreeSpace[MaxCount];

        public FreeSpaceDatabase() {
            for (var i = 0; i < FreeSpaces.Length; ++i) {
                FreeSpaces[i].BlockInfoIndex = -1;
            }
        }

        public void Register(int blockInfoIndex, long offset, long size) {
            if (size < Helpers.MinFreeSuballocationSizeToRegister) {
                return;
            }

            var bestIndex = -1;
            for (var i = 0; i < FreeSpaces.Length; ++i) {
                ref var space = ref FreeSpaces[i];

                if (space.BlockInfoIndex == -1) {
                    bestIndex = i;
                    break;
                }

                if (space.Size < size && (bestIndex == -1 || space.Size < FreeSpaces[bestIndex].Size)) {
                    bestIndex = i;
                }
            }

            if (bestIndex != -1) {
                ref var bestSpace = ref FreeSpaces[bestIndex];

                bestSpace.BlockInfoIndex = blockInfoIndex;
                bestSpace.Offset = offset;
                bestSpace.Size = size;
            }
        }

        public bool Fetch(long alignment, long size, out int blockInfoIndex, out long destOffset) {
            var bestIndex = -1;
            long bestFreeSpaceAfter = 0;

            for (var i = 0; i < FreeSpaces.Length; ++i) {
                ref var space = ref FreeSpaces[i];

                if (space.BlockInfoIndex == -1) {
                    break;
                }

                var tmpOffset = Helpers.AlignUp(space.Offset, alignment);

                if (tmpOffset + size <= space.Offset + space.Size) {
                    var freeSpaceAfter = space.Offset + space.Size - (tmpOffset + size);

                    if (bestIndex == -1 || freeSpaceAfter > bestFreeSpaceAfter) {
                        bestIndex = i;
                        bestFreeSpaceAfter = freeSpaceAfter;
                    }
                }
            }

            if (bestIndex != -1) {
                ref var bestSpace = ref FreeSpaces[bestIndex];

                blockInfoIndex = bestSpace.BlockInfoIndex;
                destOffset = Helpers.AlignUp(bestSpace.Offset, alignment);

                if (bestFreeSpaceAfter >= Helpers.MinFreeSuballocationSizeToRegister) {
                    var alignmentPlusSize = destOffset - bestSpace.Offset + size;

                    bestSpace.Offset += alignmentPlusSize;
                    bestSpace.Size -= alignmentPlusSize;
                } else {
                    bestSpace.BlockInfoIndex = -1;
                }

                return true;
            }

            blockInfoIndex = default;
            destOffset = default;
            return false;
        }

        struct FreeSpace {
            public int BlockInfoIndex;
            public long Offset, Size;
        }
    }
}
