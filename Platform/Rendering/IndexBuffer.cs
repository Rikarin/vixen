using Rin.Platform.Vulkan;

namespace Rin.Platform.Rendering;

public abstract class IndexBuffer : IDisposable {
    public abstract int Count { get; }
    public abstract int Size { get; }
    // public abstract RendererId RendererId { get; }

    // public abstract void Bind();
    public abstract void SetData(ReadOnlySpan<byte> data);
    public abstract void Dispose();
    
    // public static IndexBuffer Create(int size) {
    //     switch (RendererApi.CurrentApi) {
    //         case RendererApi.Api.None: throw new NotImplementedException();
    //         // case RendererApi.Api.OpenGl: return new OpenGLIndexBuffer(indices);
    //         default: throw new ArgumentOutOfRangeException();
    //     }
    // }

    public static IndexBuffer Create(ReadOnlySpan<byte> data) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.Vulkan: return new VulkanIndexBuffer(data);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
