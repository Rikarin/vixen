namespace Vixen.Platform.Vulkan;

sealed class DescriptorSetManagerOptions {
    public VulkanShader Shader { get; set; }
    public string DebugName { get; set; }
    public int StartSet { get; set; }
    public int EndSet { get; set; } = 3;
    public bool DefaultResources { get; set; }
}
