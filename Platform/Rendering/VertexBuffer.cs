using Rin.Core.Abstractions;
using Rin.Platform.Vulkan;

namespace Rin.Platform.Rendering;

public abstract class VertexBuffer : IDisposable {
    public RendererId RendererId { get; protected set; }
    public int Size { get; protected set; }
    
    // TODO: consider using this API instead
    // public abstract void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;
    
    public abstract void SetData(ReadOnlySpan<byte> data);
    public abstract void SetData_RT(ReadOnlySpan<byte> data);
    public abstract void Dispose();

    
    // TODO: "usage" parameter is not used anywhere
    public static VertexBuffer Create(int size, VertexBufferUsage usage = VertexBufferUsage.Dynamic) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanVertexBuffer(size, usage);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static VertexBuffer Create(ReadOnlySpan<byte> data, VertexBufferUsage usage = VertexBufferUsage.Static) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanVertexBuffer(data, usage);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}