using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

sealed class VulkanCommandPool : IDisposable {
    public CommandPool GraphicsCommandPool { get; }
    public CommandPool ComputeCommandPool { get; }

    public unsafe VulkanCommandPool() {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;
        var vulkanDevice = device.VkLogicalDevice;

        var cmdPoolInfo = new CommandPoolCreateInfo {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = device.PhysicalDevice.QueueFamilyIndices.Graphics!.Value,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        if (vk.CreateCommandPool(vulkanDevice, cmdPoolInfo, null, out var gPool) != Result.Success) {
            throw new("Failed to create Graphics Command Pool");
        }

        cmdPoolInfo.QueueFamilyIndex = device.PhysicalDevice.QueueFamilyIndices.Compute!.Value;
        if (vk.CreateCommandPool(vulkanDevice, cmdPoolInfo, null, out var cPool) != Result.Success) {
            throw new("Failed to create Compute Command Pool");
        }

        GraphicsCommandPool = gPool;
        ComputeCommandPool = cPool;
    }

    public CommandBuffer AllocateCommandBuffer(bool begin, bool compute = false) {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;

        var allocInfo = new CommandBufferAllocateInfo {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = compute ? ComputeCommandPool : GraphicsCommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        if (vk.AllocateCommandBuffers(device.VkLogicalDevice, allocInfo, out var commandBuffer) != Result.Success) {
            throw new("Failed to allocate command buffer");
        }

        if (begin) {
            var beginInfo = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo };
            if (vk.BeginCommandBuffer(commandBuffer, beginInfo) != Result.Success) {
                throw new("Failed to begin command buffer");
            }
        }

        return commandBuffer;
    }

    public void FlushCommandBuffer(CommandBuffer commandBuffer) =>
        FlushCommandBuffer(commandBuffer, VulkanContext.CurrentDevice.GraphicsQueue);

    public unsafe void FlushCommandBuffer(CommandBuffer commandBuffer, Queue queue) {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;

        if (vk.EndCommandBuffer(commandBuffer) != Result.Success) {
            throw new("Failed to end command buffer");
        }

        var fenceCreateInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo, Flags = 0 };
        var submitInfo = new SubmitInfo {
            SType = StructureType.SubmitInfo, CommandBufferCount = 1, PCommandBuffers = &commandBuffer
        };

        vk.CreateFence(device.VkLogicalDevice, in fenceCreateInfo, null, out var fence);
        lock (this) {
            vk.QueueSubmit(queue, 1, submitInfo, fence);
        }

        vk.WaitForFences(device.VkLogicalDevice, 1, fence, true, 100000000000);
        vk.DestroyFence(device.VkLogicalDevice, fence, null);
        vk.FreeCommandBuffers(device.VkLogicalDevice, GraphicsCommandPool, 1, commandBuffer);
    }

    public unsafe void Dispose() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var vk = VulkanContext.Vulkan;

        vk.DestroyCommandPool(device, GraphicsCommandPool, null);
        vk.DestroyCommandPool(device, ComputeCommandPool, null);
    }
}
