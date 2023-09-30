using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan;

sealed class VulkanIndexBuffer : IndexBuffer {
    // readonly byte[] localBuffer;
    Allocation allocation;

    public Buffer VkBuffer { get; private set; }

    public override int Count => Size / sizeof(int);
    public override int Size { get; }

    public unsafe VulkanIndexBuffer(ReadOnlySpan<byte> data) {
        var localBuffer = data.ToArray();
        Size = localBuffer.Length;

        Renderer.Submit(
            () => {
                var bufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
                    Size = (uint)Size,
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

                var indexBufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
                    Size = (uint)Size,
                    Usage = BufferUsageFlags.TransferDstBit | BufferUsageFlags.IndexBufferBit
                };

                allocation = VulkanAllocator.AllocateBuffer(
                    indexBufferCreateInfo,
                    MemoryUsage.GPU_Only,
                    out var buffer
                );
                VkBuffer = buffer;

                var device = VulkanContext.CurrentDevice;
                var copyCommand = device.GetCommandBuffer(true);
                var copyRegion = new BufferCopy { Size = (uint)Size };

                VulkanContext.Vulkan.CmdCopyBuffer(copyCommand, stagingBuffer, buffer, 1, copyRegion);
                device.FlushCommandBuffer(copyCommand);
                VulkanAllocator.DestroyBuffer(stagingBuffer, stagingAllocation);
            }
        );
    }

    public override void SetData(ReadOnlySpan<byte> data) {
        // data.CopyTo(localBuffer);
        // Renderer.Submit(() => SetData_RT(localBuffer));
        throw new NotImplementedException();
    }

    // public override unsafe void SetData_RT(ReadOnlySpan<byte> data) {
        // TODO: this will probably fail when used with GPU_ONLY buffer
        // var destData = new Span<byte>(allocation.Map().ToPointer(), data.Length);
        // data.CopyTo(destData);
        // allocation.Unmap();
    // }

    public override void Dispose() {
        Renderer.SubmitDisposal(() => VulkanAllocator.DestroyBuffer(VkBuffer, allocation));
    }
}
