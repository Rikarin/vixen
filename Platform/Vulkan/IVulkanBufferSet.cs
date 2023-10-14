namespace Rin.Platform.Vulkan;

interface IVulkanBufferSet {
    IVulkanBuffer GetVulkanBuffer(int frame);
}
