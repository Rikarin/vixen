using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Vulkan.Extensions.NV;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

sealed class VulkanDevice : IDisposable {
    readonly PhysicalDeviceFeatures enabledFeatures;
    readonly ThreadLocal<VulkanCommandPool> commandPool = new(() => new());

    bool enableDebugMarkers;

    public Queue GraphicsQueue { get; }
    public Queue ComputeQueue { get; }
    public Device VkLogicalDevice { get; }
    public VulkanPhysicalDevice PhysicalDevice { get; }

    public unsafe VulkanDevice(VulkanPhysicalDevice physicalDevice, PhysicalDeviceFeatures enabledFeatures) {
        this.enabledFeatures = enabledFeatures;
        PhysicalDevice = physicalDevice;

        if (!PhysicalDevice.IsExtensionSupported(KhrSwapchain.ExtensionName)) {
            Log.Fatal("Swapchain not supported");
        }

        var deviceExtensions = new List<string> { KhrSwapchain.ExtensionName };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            deviceExtensions.Add("VK_KHR_portability_subset");
        }

        if (PhysicalDevice.IsExtensionSupported(NVDeviceDiagnosticCheckpoints.ExtensionName)) {
            deviceExtensions.Add(NVDeviceDiagnosticCheckpoints.ExtensionName);
        }

        // This seems to be not even exported by Silk.NET
        if (PhysicalDevice.IsExtensionSupported("VK_NV_device_diagnostics_config")) {
            deviceExtensions.Add("VK_NV_device_diagnostics_config");
        }

        if (PhysicalDevice.IsExtensionSupported(ExtDebugMarker.ExtensionName)) {
            deviceExtensions.Add(ExtDebugMarker.ExtensionName);
            enableDebugMarkers = true;
        }

        // TODO: initialization Aftermath if requested

        var pQueueCreateInfos = PhysicalDevice.queueCreateInfos;
        using var handle = VulkanUtils.Alloc<DeviceQueueCreateInfo>(pQueueCreateInfos.Count, out var queueCreateInfos);

        for (var i = 0; i < pQueueCreateInfos.Count; i++) {
            queueCreateInfos[i] = pQueueCreateInfos[i];
        }

        var deviceCreateInfo = new DeviceCreateInfo {
            SType = StructureType.DeviceCreateInfo,
            // Flags = portability
            QueueCreateInfoCount = (uint)pQueueCreateInfos.Count,
            PQueueCreateInfos = queueCreateInfos,
            PEnabledFeatures = &enabledFeatures
        };

        if (deviceExtensions.Count > 0) {
            deviceCreateInfo.EnabledExtensionCount = (uint)deviceExtensions.Count;
            deviceCreateInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions.ToArray());
        }

        var vk = VulkanContext.Vulkan;
        if (vk.CreateDevice(PhysicalDevice.VkPhysicalDevice, deviceCreateInfo, null, out var device)
            != Result.Success) {
            throw new("Failed to create logical device");
        }

        var queueFamilies = PhysicalDevice.QueueFamilyIndices;
        vk.GetDeviceQueue(device, queueFamilies.Graphics!.Value, 0, out var graphicsQueue);
        vk.GetDeviceQueue(device, queueFamilies.Compute!.Value, 0, out var computeQueue);

        VkLogicalDevice = device;
        GraphicsQueue = graphicsQueue;
        ComputeQueue = computeQueue;

        SilkMarshal.Free((nint)deviceCreateInfo.PpEnabledExtensionNames);
    }

    public CommandBuffer GetCommandBuffer(bool begin, bool compute = false) =>
        commandPool.Value!.AllocateCommandBuffer(begin, compute);

    public void FlushCommandBuffer(CommandBuffer commandBuffer) => commandPool.Value!.FlushCommandBuffer(commandBuffer);

    public void FlushCommandBuffer(CommandBuffer commandBuffer, Queue queue) =>
        commandPool.Value!.FlushCommandBuffer(commandBuffer, queue);

    public CommandBuffer CreateSecondaryCommandBuffer(string debugName) {
        var allocateInfo = new CommandBufferAllocateInfo {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool.Value!.GraphicsCommandPool,
            Level = CommandBufferLevel.Secondary,
            CommandBufferCount = 1
        };

        var vk = VulkanContext.Vulkan;
        if (vk.AllocateCommandBuffers(VkLogicalDevice, in allocateInfo, out var commandBuffer) != Result.Success) {
            throw new("Failed to allocate secondary command buffer");
        }

        VulkanUtils.SetDebugObjectName(ObjectType.CommandBuffer, debugName, commandBuffer.Handle);
        return commandBuffer;
    }

    public unsafe void Dispose() {
        var vk = VulkanContext.Vulkan;

        commandPool.Dispose();
        vk.DeviceWaitIdle(VkLogicalDevice);
        vk.DestroyDevice(VkLogicalDevice, null);
    }
}
