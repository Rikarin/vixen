using Rin.Platform.Silk;

namespace Rin.Platform.Renderer;

public abstract class VertexBuffer : IDisposable {
    /*internal*/ public BufferLayout Layout { get; set; }

    public abstract void Bind();
    public abstract void Unbind();
    public abstract void SetData(ReadOnlySpan<uint> data);

    public static VertexBuffer Create(uint size) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.OpenGl: return new OpenGLVertexBuffer(size);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static VertexBuffer Create(ReadOnlySpan<float> vertices) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.OpenGl: return new OpenGLVertexBuffer(vertices);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Dispose() { }
}
