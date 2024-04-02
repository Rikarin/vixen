using Silk.NET.Vulkan;
using System.Runtime.InteropServices;
using Vixen.Platform.Common.Rendering;
using Vixen.Platform.Vulkan.Allocator;
using Vixen.Rendering;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Vixen.Platform.Vulkan;

sealed class VulkanUniformBuffer : IUniformBuffer, IVulkanBuffer {
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
    
    public void SetData<T>(ReadOnlySpan<T> data) where T : struct {
        var payload = MemoryMarshal.AsBytes(data);
        payload.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT<byte>(localBuffer));
    }

    public void SetData<T>(T data) where T : struct {
        var payload = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref data, 1));
        payload.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT<byte>(localBuffer));
    }
    
    public unsafe void SetData_RT<T>(ReadOnlySpan<T> data) where T : struct {
        var payload = MemoryMarshal.AsBytes(data);
        var destData = new Span<byte>(allocation!.Map().ToPointer(), payload.Length);
        
        payload.CopyTo(destData);
        allocation.Unmap();
    }
    
    public unsafe void SetData_RT<T>(T data) where T : struct {
        var payload = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref data, 1));
        var destData = new Span<byte>(allocation!.Map().ToPointer(), payload.Length);
        
        payload.CopyTo(destData);
        allocation.Unmap();
    }

    public void Dispose() {
        Renderer.SubmitDisposal(() => VulkanAllocator.DestroyBuffer(vkBuffer, allocation!));
    }
}
