using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

public static class VulkanSampler {
    public static ResourceAllocationCounts Resources { get; } = new();

    public static unsafe Sampler CreateSampler(SamplerCreateInfo createInfo) {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        VulkanContext.Vulkan.CreateSampler(device, createInfo, null, out var sampler).EnsureSuccess();

        Resources.Samplers++;
        return sampler;
    }

    public static unsafe void DestroySampler(Sampler sampler) {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        VulkanContext.Vulkan.DestroySampler(device, sampler, null);
        Resources.Samplers--;
    }
}

// TODO: log these in EventSource instead?
public class ResourceAllocationCounts {
    public int Samplers { get; set; }
}
