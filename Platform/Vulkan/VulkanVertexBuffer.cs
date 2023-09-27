using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan;

sealed class VulkanVertexBuffer : VertexBuffer {
    readonly byte[] localBuffer;
    Allocation allocation;

    public Buffer VkBuffer { get; private set; }

    public unsafe VulkanVertexBuffer(int size, VertexBufferUsage usage) {
        RendererId = new(0);
        localBuffer = new byte[size];

        Renderer.Submit(
            () => {
                var bufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
                    Size = (uint)size, Usage = BufferUsageFlags.VertexBufferBit
                };

                allocation = VulkanAllocator.AllocateBuffer(bufferCreateInfo, MemoryUsage.CPU_To_GPU, out var buffer);
                VkBuffer = buffer;
            }
        );
    }

    public unsafe VulkanVertexBuffer(ReadOnlySpan<byte> data, VertexBufferUsage usage) {
        RendererId = new(0);
        localBuffer = data.ToArray();

        Renderer.Submit(
            () => {
                var bufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
                    Size = (uint)localBuffer.Length,
                    Usage = BufferUsageFlags.TransferSrcBit,
                    SharingMode = SharingMode.Exclusive
                };

                var stagingAllocation = VulkanAllocator.AllocateBuffer(
                    bufferCreateInfo,
                    MemoryUsage.CPU_To_GPU,
                    out var stagingBuffer
                );
                var destData = new Span<byte>(stagingAllocation.Map().ToPointer(), localBuffer.Length);
                localBuffer.CopyTo(destData);
                stagingAllocation.Unmap();

                var vertexBufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
                    Size = (uint)localBuffer.Length,
                    Usage = BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit
                };

                allocation = VulkanAllocator.AllocateBuffer(
                    vertexBufferCreateInfo,
                    MemoryUsage.GPU_Only,
                    out var buffer
                );
                VkBuffer = buffer;

                var device = VulkanContext.CurrentDevice;
                var copyCommand = device.GetCommandBuffer(true);
                var copyRegion = new BufferCopy { Size = (uint)localBuffer.Length };

                VulkanContext.Vulkan.CmdCopyBuffer(copyCommand, stagingBuffer, buffer, 1, copyRegion);
                VulkanAllocator.DestroyBuffer(stagingBuffer, stagingAllocation);
            }
        );
    }

    public override void SetData(ReadOnlySpan<byte> data) {
        data.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT(localBuffer));
    }

    public override unsafe void SetData_RT(ReadOnlySpan<byte> data) {
        // TODO: this will probably fail when used with GPU_ONLY buffer
        var destData = new Span<byte>(allocation.Map().ToPointer(), data.Length);
        data.CopyTo(destData);
        allocation.Unmap();
    }

    public override void Dispose() {
        // TODO: not sure if we can do it here or we need to call Renderer.SubmitResourceFree
        VulkanAllocator.DestroyBuffer(VkBuffer, allocation);
        base.Dispose();
    }
}
