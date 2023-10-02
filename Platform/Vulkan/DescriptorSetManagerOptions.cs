namespace Rin.Platform.Vulkan;

public class DescriptorSetManagerOptions {
    // TODO vulkan shader
    public string DebugName { get; set; }
    public int StartSet { get; set; }
    public int StopSet { get; set; }
    public bool DefaultResources { get; set; }
}