using Rin.Platform.Vulkan.Allocator;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan;

static class VulkanAllocator {
    static VulkanMemoryAllocator allocator;

    public static void Init() {
        allocator = new(
            new() {
                VulkanAPIVersion = new(1, 2, 0),
                VulkanAPIObject = VulkanContext.Vulkan,
                Instance = VulkanContext.Vulkan.CurrentInstance!.Value,
                PhysicalDevice = VulkanContext.CurrentDevice.PhysicalDevice.VkPhysicalDevice,
                LogicalDevice = VulkanContext.CurrentDevice.VkLogicalDevice
            }
        );
    }

    public static void Shutdown() {
        allocator.Dispose();
        allocator = null!;
    }

    public static Allocation AllocateBuffer(BufferCreateInfo bufferCreateInfo, MemoryUsage memoryUsage, out Buffer buffer) {
        var allocationCreateInfo = new AllocationCreateInfo { Usage = memoryUsage };

        buffer = allocator.CreateBuffer(bufferCreateInfo, allocationCreateInfo, out var allocation);
        // TODO: can allocation be null?
        // TODO: tracking and logging stuff
        return allocation;
    }
    
    public static Allocation AllocateImage(ImageCreateInfo imageCreateInfo, MemoryUsage memoryUsage, out Image image) {
        var allocationCreateInfo = new AllocationCreateInfo { Usage = memoryUsage };
        
        image = allocator.CreateImage(imageCreateInfo, allocationCreateInfo, out var allocation);
        // TODO: can allocation be null?
        // TODO: tracking and logging stuff
        return allocation;
    }

    public static void Free(Allocation allocation) {
        allocation.Dispose();
        // TODO: tracking
    }

    public static unsafe void DestroyImage(Image image, Allocation allocation) {
        VulkanContext.Vulkan.DestroyImage(allocation.Allocator.Device, image, null);
        allocation.Dispose();
    }
    
    public static unsafe void DestroyBuffer(Buffer buffer, Allocation allocation) {
        VulkanContext.Vulkan.DestroyBuffer(allocation.Allocator.Device, buffer, null);
        allocation.Dispose();
    }

    
    // public nint MapMemory(Allocation allocation) {
    //     return allocation.Map();
    // }
    //
    // public void UnmapMemory(Allocation allocation) {
    //     allocation.Unmap();
    // }
}
