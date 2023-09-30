using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan; 

public sealed class VulkanUniformBuffer : UniformBuffer {
    readonly byte[] localBuffer;
    readonly Allocation? allocation;
    readonly Buffer vkBuffer;
    
    public DescriptorBufferInfo DescriptorBufferInfo { get; }

    public unsafe VulkanUniformBuffer(int size) {
        localBuffer = new byte[size];
        var bufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
            Usage = BufferUsageFlags.UniformBufferBit, Size = (uint)size
        };

        allocation = VulkanAllocator.AllocateBuffer(bufferCreateInfo, MemoryUsage.CPU_To_GPU, out vkBuffer);
        DescriptorBufferInfo = new() { Buffer = vkBuffer, Range = (uint)size };
    }
    
    public override void SetData(ReadOnlySpan<byte> data) {
        data.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT(localBuffer));
    }

    public override unsafe void SetData_RT(ReadOnlySpan<byte> data) {
        var destData = new Span<byte>(allocation!.Map().ToPointer(), data.Length);
        data.CopyTo(destData);
        allocation.Unmap();
    }

    public override void Dispose() {
        Renderer.SubmitDisposal(() => VulkanAllocator.DestroyBuffer(vkBuffer, allocation!));
    }
}