namespace Vixen.Platform.Vulkan;

interface IVulkanBufferSet {
    IVulkanBuffer GetVulkanBuffer(int frame);
}
