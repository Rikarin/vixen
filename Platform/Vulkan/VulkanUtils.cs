using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

static class VulkanUtils {
    public static unsafe MemoryHandle Alloc<T>(int size, out T* buffer) where T : unmanaged {
        var handle = new Memory<byte>(new byte[size * sizeof(T)]).Pin();
        buffer = (T*)handle.Pointer;
        return handle;
    }

    public static unsafe MemoryHandle Alloc<T>(uint size, out T* buffer) where T : unmanaged {
        var handle = new Memory<byte>(new byte[size * sizeof(T)]).Pin();
        buffer = (T*)handle.Pointer;
        return handle;
    }

    public static void SetDebugObjectName(ObjectType objectType, string name, nint handle) {
        SetDebugObjectName(objectType, name, (ulong)handle);
    }

    public static unsafe void SetDebugObjectName(ObjectType objectType, string name, ulong handle) {
        var vk = VulkanContext.Vulkan;
        if (!vk.TryGetInstanceExtension(vk.CurrentInstance!.Value, out ExtDebugUtils utils)) {
            return;
        }

        var nameInfo = new DebugUtilsObjectNameInfoEXT(StructureType.DebugUtilsObjectNameInfoExt) {
            ObjectType = objectType, PObjectName = (byte*)Marshal.StringToHGlobalAnsi(name), ObjectHandle = handle
        };

        utils.SetDebugUtilsObjectName(VulkanContext.CurrentDevice.VkLogicalDevice, nameInfo).EnsureSuccess();
        Marshal.FreeHGlobal((IntPtr)nameInfo.PObjectName);
    }

    public static unsafe void InsertImageMemoryBarrier(
        CommandBuffer commandBuffer,
        Image image,
        AccessFlags srcAccessMask,
        AccessFlags dstAccessMask,
        ImageLayout oldImageLayout,
        ImageLayout newImageLayout,
        PipelineStageFlags srcStakeMask,
        PipelineStageFlags dstStageMask,
        ImageSubresourceRange subresourceRange
    ) {
        var imageMemoryBarrier = new ImageMemoryBarrier(StructureType.ImageMemoryBarrier) {
            SrcQueueFamilyIndex = ~0U,
            DstQueueFamilyIndex = ~0U,
            SrcAccessMask = srcAccessMask,
            DstAccessMask = dstAccessMask,
            OldLayout = oldImageLayout,
            NewLayout = newImageLayout,
            Image = image,
            SubresourceRange = subresourceRange
        };

        VulkanContext.Vulkan.CmdPipelineBarrier(
            commandBuffer,
            srcStakeMask,
            dstStageMask,
            DependencyFlags.None,
            0,
            null,
            0,
            null,
            1,
            imageMemoryBarrier
        );
    }
}
