using Silk.NET.Vulkan;
using System.Diagnostics;

namespace Rin.Platform.Vulkan.Allocator;

class DedicatedAllocation : Allocation {
    internal DeviceMemory memory;
    internal IntPtr mappedData;

    public override DeviceMemory DeviceMemory => memory;

    public override long Offset {
        get => 0;
        internal set => throw new InvalidOperationException();
    }

    public override IntPtr MappedData => mapCount != 0 ? mappedData : default;

    internal override bool CanBecomeLost => false;

    public DedicatedAllocation(
        VulkanMemoryAllocator allocator,
        int memTypeIndex,
        DeviceMemory memory,
        SuballocationType suballocType,
        IntPtr mappedData,
        long size
    ) : base(allocator, 0) {
        this.memory = memory;
        this.mappedData = mappedData;
        memoryTypeIndex = memTypeIndex;
    }

    public void CalcStatsInfo(out StatInfo stats) {
        StatInfo.Init(out stats);
        stats.BlockCount = 1;
        stats.AllocationCount = 1;
        stats.UsedBytes = Size;
        stats.AllocationSizeMin = stats.AllocationSizeMax = Size;
    }

    public override IntPtr Map() {
        var res = DedicatedAllocMap(out var pData);

        if (res != Result.Success) {
            throw new MapMemoryException(res);
        }

        return pData;
    }

    public override void Unmap() {
        DedicatedAllocUnmap();
    }

    internal unsafe Result DedicatedAllocMap(out IntPtr pData) {
        if (mapCount != 0) {
            if ((mapCount & int.MaxValue) < int.MaxValue) {
                Debug.Assert(mappedData != default);

                pData = mappedData;
                mapCount += 1;

                return Result.Success;
            }

            throw new InvalidOperationException("Dedicated allocation mapped too many times simultaneously");
        }

        pData = default;

        IntPtr tmp;
        var res = VkApi.MapMemory(Allocator.Device, memory, 0, Vk.WholeSize, 0, (void**)&tmp);

        if (res == Result.Success) {
            mappedData = tmp;
            mapCount = 1;
            pData = tmp;
        }

        return res;
    }

    internal void DedicatedAllocUnmap() {
        if ((mapCount & int.MaxValue) != 0) {
            mapCount -= 1;

            if (mapCount == 0) {
                mappedData = default;
                VkApi.UnmapMemory(Allocator.Device, memory);
            }
        } else {
            throw new InvalidOperationException("Unmapping dedicated allocation not previously mapped");
        }
    }
}
