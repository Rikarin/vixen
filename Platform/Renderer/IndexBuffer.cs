using Rin.Platform.Silk;

namespace Rin.Platform.Renderer;

public abstract class IndexBuffer : IDisposable {
    public abstract int Count { get; }

    public abstract void Bind();
    public abstract void Unbind();

    public static IndexBuffer Create(ReadOnlySpan<int> indices) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.OpenGl: return new OpenGLIndexBuffer(indices);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Dispose() { }
}
