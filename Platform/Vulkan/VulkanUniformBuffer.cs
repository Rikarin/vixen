using Rin.Platform.Rendering;

namespace Rin.Platform.Vulkan; 

public sealed class VulkanUniformBuffer : UniformBuffer {

    public VulkanUniformBuffer(int size) {
        
    }
    
    public override void SetData(ReadOnlySpan<byte> data) {
        throw new NotImplementedException();
    }

    public override void SetData_RT(ReadOnlySpan<byte> data) {
        throw new NotImplementedException();
    }
}
