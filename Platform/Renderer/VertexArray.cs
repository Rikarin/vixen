using Rin.Platform.Silk;

namespace Rin.Platform.Renderer;

public abstract class VertexArray : IDisposable {
    public abstract IReadOnlyList<VertexBuffer> VertexBuffers { get; }
    public IndexBuffer IndexBuffer { get; protected set; }

    public abstract void Bind();
    public abstract void Unbind();
    public abstract void AddVertexBuffer(VertexBuffer buffer);
    public abstract void SetIndexBuffer(IndexBuffer buffer);

    public static VertexArray Create() {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.OpenGl: return new OpenGLVertexArray();
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Dispose() { }
}
