using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Rin.Rendering;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan;

sealed class VulkanVertexBuffer : IVertexBuffer {
    readonly byte[] localBuffer;
    Allocation allocation;

    public Buffer VkBuffer { get; private set; }

    public RendererId RendererId { get; }
    public int Size { get; }

    public unsafe VulkanVertexBuffer(int size, VertexBufferUsage usage) {
        Size = size;
        RendererId = new(0);
        localBuffer = new byte[size];

        if (usage != VertexBufferUsage.Dynamic) {
            throw new NotImplementedException();
        }

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
        Size = data.Length;
        RendererId = new(0);
        localBuffer = data.ToArray();

        if (usage != VertexBufferUsage.Static) {
            throw new NotImplementedException();
        }

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
                device.FlushCommandBuffer(copyCommand);
                VulkanAllocator.DestroyBuffer(stagingBuffer, stagingAllocation);
            }
        );
    }

    public void SetData(ReadOnlySpan<byte> data) {
        data.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT(localBuffer));
    }

    public unsafe void SetData_RT(ReadOnlySpan<byte> data) {
        // TODO: this will probably fail when used with GPU_ONLY buffer
        var destData = new Span<byte>(allocation.Map().ToPointer(), data.Length);
        data.CopyTo(destData);
        allocation.Unmap();
    }

    public void Dispose() {
        Renderer.SubmitDisposal(() => VulkanAllocator.DestroyBuffer(VkBuffer, allocation));
    }
}
