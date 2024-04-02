using Silk.NET.Vulkan;

namespace Vixen.Platform.Vulkan;

interface IVulkanBuffer {
    DescriptorBufferInfo DescriptorBufferInfo { get; }
}