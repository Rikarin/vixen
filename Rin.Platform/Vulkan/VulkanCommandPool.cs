using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

sealed class VulkanCommandPool : IDisposable {
    public CommandPool GraphicsCommandPool { get; }
    public CommandPool ComputeCommandPool { get; }

    public unsafe VulkanCommandPool() {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;
        var vulkanDevice = device.VkLogicalDevice;

        var cmdPoolInfo = new CommandPoolCreateInfo(StructureType.CommandPoolCreateInfo) {
            QueueFamilyIndex = device.PhysicalDevice.QueueFamilyIndices.Graphics!.Value,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };
        
        vk.CreateCommandPool(vulkanDevice, cmdPoolInfo, null, out var gPool).EnsureSuccess();

        cmdPoolInfo.QueueFamilyIndex = device.PhysicalDevice.QueueFamilyIndices.Compute!.Value;
        vk.CreateCommandPool(vulkanDevice, cmdPoolInfo, null, out var cPool).EnsureSuccess();

        GraphicsCommandPool = gPool;
        ComputeCommandPool = cPool;
    }

    public unsafe CommandBuffer AllocateCommandBuffer(bool begin, bool compute = false) {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;

        var allocInfo = new CommandBufferAllocateInfo {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = compute ? ComputeCommandPool : GraphicsCommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        vk.AllocateCommandBuffers(device.VkLogicalDevice, allocInfo, out var commandBuffer).EnsureSuccess();

        if (begin) {
            var beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            vk.BeginCommandBuffer(commandBuffer, beginInfo).EnsureSuccess();
        }

        return commandBuffer;
    }

    public void FlushCommandBuffer(CommandBuffer commandBuffer) =>
        FlushCommandBuffer(commandBuffer, VulkanContext.CurrentDevice.GraphicsQueue);

    public unsafe void FlushCommandBuffer(CommandBuffer commandBuffer, Queue queue) {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;

        vk.EndCommandBuffer(commandBuffer).EnsureSuccess();

        var fenceCreateInfo = new FenceCreateInfo(StructureType.FenceCreateInfo);
        var submitInfo = new SubmitInfo(StructureType.SubmitInfo) {
            CommandBufferCount = 1, PCommandBuffers = &commandBuffer
        };

        vk.CreateFence(device.VkLogicalDevice, in fenceCreateInfo, null, out var fence).EnsureSuccess();
        lock (this) {
            vk.QueueSubmit(queue, 1, submitInfo, fence).EnsureSuccess();
        }

        vk.WaitForFences(device.VkLogicalDevice, 1, fence, true, 100000000000).EnsureSuccess();
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
