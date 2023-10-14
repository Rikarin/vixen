using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

interface IVulkanBuffer {
    DescriptorBufferInfo DescriptorBufferInfo { get; }
}