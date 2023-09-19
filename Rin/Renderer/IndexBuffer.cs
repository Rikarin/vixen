using Editor.Platform.Silk;

namespace Editor.Renderer;

public abstract class IndexBuffer : IDisposable {
    public abstract int Count { get; }

    public abstract void Bind();
    public abstract void Unbind();

    public static IndexBuffer Create(ReadOnlySpan<uint> indices) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.OpenGl: return new OpenGLIndexBuffer(indices);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Dispose() { }
}