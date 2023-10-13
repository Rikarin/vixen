using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Rin.Rendering;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan;

sealed class VulkanUniformBuffer : IUniformBuffer {
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

    public void SetData(ReadOnlySpan<byte> data) {
        data.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT(localBuffer));
    }

    public unsafe void SetData_RT(ReadOnlySpan<byte> data) {
        var destData = new Span<byte>(allocation!.Map().ToPointer(), data.Length);
        data.CopyTo(destData);
        allocation.Unmap();
    }

    public void Dispose() {
        Renderer.SubmitDisposal(() => VulkanAllocator.DestroyBuffer(vkBuffer, allocation!));
    }
}
